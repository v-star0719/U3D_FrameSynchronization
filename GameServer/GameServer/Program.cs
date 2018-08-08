using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameServer
{
	class PlayerInfo
	{
		public Socket Socket;
		public int PlayerId;
		public byte[] ReadBuffer = new byte[512];
		public byte[] WriteBuffer = new byte[512];

		public byte[] AnalysisBuffer = new byte[512];
		public byte[] AnalysisTempBuffer = new byte[512];
		public int AnalysisBufferLength = 0;

		public bool IsDisconnected;

		public OperationData OperationData;
		public bool IsOperationReceived = false;

		public NetBuff NetReadBuff = new NetBuff();
		public NetBuff NetWriteBuff = new NetBuff();

		public int CurFrame = 1;
	}

	class Program
	{
		private static Socket tcpSocket;

		private static List<PlayerInfo> playerList = new List<PlayerInfo>();
		private static int curPlayerId = 1;

		private static NetBuff netSendBuff = new NetBuff();
		private static NetBuff netReceiveBuff = new NetBuff();

		private static int curFrame = 0;
		private static char inputChar;

		private static bool isStart = false;

		//强制完全同步的时候，等所有客户端都跑完一帧后继续下一帧
		//否则的话服务器自己跑自己的，客户端追赶服务器的脚步
		private static bool forceSync = false;

		private static List<FrameData> historyFrameDatas = new List<FrameData>(200);//记录200帧的操作

		private const int FORWARD_FRAME_COUNT = 5;//服务器超前的帧数，超过这个数量就等待客户端跟上来
		private const int DELTA_TIME = 25;//单位ms

		//采用异步模式
		static void Main(string[] args)
		{
			Thread t = new Thread(ServerCheckAliveThread);
			t.Start();
			t = new Thread(ServerSyncThread);
			t.Start();


			tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			//启动服务器
			IPAddress ip = IPAddress.Parse("127.0.0.1");
			EndPoint endPoint = new IPEndPoint(ip, 6123);
			tcpSocket.Bind(endPoint);
			Console.WriteLine("服务器端启动完成，输入a开始同步");

			tcpSocket.Listen(100);//最多可以接收100个客户端请求
			tcpSocket.BeginAccept(OnConnected, null);

			while(true)
			{
				var key = (char)Console.Read();
				if(char.IsLetter(key))
				{
					inputChar = key;
				}

				Thread.Sleep(1000);
			}

			Console.ReadLine();
		}

		public static void OnConnected(IAsyncResult ar)
		{
			var socket = tcpSocket.EndAccept(ar);


			//新建一个角色
			PlayerInfo playerInfo = new PlayerInfo();
			playerInfo.Socket = socket;
			playerInfo.PlayerId = curPlayerId++;
			playerInfo.IsOperationReceived = true;//第一帧是服务器直接发过去的空帧
			playerList.Add(playerInfo);
			Console.WriteLine("收到客户端连接，玩家id = " + playerInfo.PlayerId);

			socket.BeginReceive(playerInfo.ReadBuffer, 0, playerInfo.ReadBuffer.Length, SocketFlags.None, OnReceive, playerInfo);


			//发送玩家信息
			netSendBuff.Reset();
			NetSCSelfInfoMsg msg = new NetSCSelfInfoMsg();
			msg.PlayerId = playerInfo.PlayerId;
			msg.Write(netSendBuff);
			socket.Send(netSendBuff.DataBuffer);

			//玩家列表消息
			NetSCPlayerListMsg msg2 = new NetSCPlayerListMsg();
			for(int i = 0; i < playerList.Count; i++)
			{
				msg2.PlayerList.PlayerList.Add(playerList[i].PlayerId);
			}
			netSendBuff.Reset();
			msg2.Write(netSendBuff);

			//给所有玩家发送玩家列表
			for(int i = 0; i < playerList.Count; i++)
			{
				try
				{
					playerList[i].Socket.Send(netSendBuff.DataBuffer);
				}
				catch(Exception e)
				{
					playerList[i].IsDisconnected = true;
				}
			}

			tcpSocket.BeginAccept(OnConnected, null);
		}

		public static void OnReceive(IAsyncResult ar)
		{
			PlayerInfo info = ar.AsyncState as PlayerInfo;
			int read = info.Socket.EndReceive(ar);

			if(read > 0)
			{
				Array.Copy(info.ReadBuffer, info.AnalysisBuffer, read);
				info.AnalysisBufferLength += read;
				AnalysisMsg(info);
			}

			try
			{
				info.Socket.BeginReceive(info.ReadBuffer, 0, info.ReadBuffer.Length, SocketFlags.None, OnReceive, info);
			}
			catch(Exception e)
			{
				info.IsDisconnected = true;
			}
		}

		private static void AnalysisMsg(PlayerInfo info)
		{
			while(info.AnalysisBufferLength > 0)
			{
				int msgLength = BitConverter.ToInt32(info.AnalysisBuffer, 0);
				//Console.WriteLine("msg length " + msgLength);
				if(msgLength <= info.AnalysisBufferLength - 4)
				{
					info.NetReadBuff.Set(info.AnalysisBuffer, 4, msgLength);
					int messageType = BitConverter.ToInt32(info.AnalysisBuffer, 4);

					if(messageType != (int)EmNetMessageType.CS_SYNCHRONIZATE)
					{
						Console.WriteLine("**** 错误，消息不是同步消息 " + messageType);
					}
					else
					{
						var msg = NetMessageFactory.GetMessage(messageType);
						if(msg != null)
						{
							msg.Read(info.NetReadBuff);
							NetCSSynchronizateMsg msgg = msg as NetCSSynchronizateMsg;
							
							if(info.CurFrame == msgg.CurFrame)
							{
								info.OperationData = msgg.OperationData;
								info.IsOperationReceived = true;
								info.CurFrame++;
							}
							else
							{
								Console.WriteLine("error: player {0} frame should be {1}, receive {2}", info.PlayerId, info.CurFrame, msgg.CurFrame);
							}

							//Console.WriteLine("receve " + msg.MessageType);
						}
						else
						{
							//Console.WriteLine("wrong message type " + messageType);
						}
					}

					int remain = info.AnalysisBufferLength - msgLength - 4;
					Array.Copy(info.AnalysisBuffer, 4 + msgLength, info.AnalysisTempBuffer, 0, remain);
					Array.Copy(info.AnalysisTempBuffer, 0, info.AnalysisBuffer, 0, remain);
					info.AnalysisBufferLength = remain;
					//Debug.Log("remian " + remain);
					//Log();
				}
				else
				{
					break;
				}
			}
		}

		static void ServerCheckAliveThread()
		{
			//使用心跳检查客户端是否存活
			while(true)
			{
				for(int i = 0; i < playerList.Count; i++)
				{
					if(playerList[i].IsDisconnected)
					{
						Console.WriteLine("玩家 {0} 断线", playerList[i].PlayerId);
						playerList.RemoveAt(i);
						i--;
					}
				}

				Thread.Sleep(10);
			}
		}

		static void ServerSyncThread()
		{
			NetBuff writeBuffer = new NetBuff();

			//使用心跳检查客户端是否存活
			while(true)
			{
				//发送同步消息
				if(!isStart)
				{
					if(inputChar == 'a' || inputChar == 'A')
					{
						NetSCSynchronizateStartMsg msg = new NetSCSynchronizateStartMsg();
						SendMsg(writeBuffer, msg);
						isStart = true;
					}

					Thread.Sleep(10);
					continue;
				}

				if(playerList.Count == 0)
				{
					isStart = false;
					Thread.Sleep(10);
					continue;
				}

				bool isOk = true && playerList.Count > 0;
				bool isWaitSomeOne = false;
				for(int i = 0; i < playerList.Count; i++)
				{
					if(!playerList[i].IsDisconnected && !playerList[i].IsOperationReceived)
					{
						isOk = false;
					}

					if(curFrame - playerList[i].CurFrame >= FORWARD_FRAME_COUNT)
					{
						isWaitSomeOne = true;
					}
				}

				if(!isWaitSomeOne && (isOk || !forceSync))
				{
					//开始一帧
					curFrame++;
					Console.WriteLine("start frame " + curFrame);

					//生成消息
					var frameData = new FrameData();
					frameData.Frame = curFrame;
					for(int i = 0; i < playerList.Count; i++)
					{
						if(!playerList[i].IsDisconnected && playerList[i].OperationData != null)
						{
							frameData.OperationDatas.Add(playerList[i].OperationData);
							playerList[i].OperationData = null;
							playerList[i].IsOperationReceived = false;
						}
					}

					historyFrameDatas.Add(frameData);
					if(historyFrameDatas.Count >= 200)
					{
						historyFrameDatas.RemoveAt(0);
					}

					SendMsg(writeBuffer, frameData);

					Console.WriteLine("send msg finish");
				}

				Thread.Sleep(DELTA_TIME);
			}
		}

		private static void SendMsg(NetBuff buffer, NetMessageBase msg)
		{
			buffer.Reset();
			msg.Write(buffer);
			for(int i = 0; i < playerList.Count; i++)
			{
				var info = playerList[i];
				if(!info.IsDisconnected)
				{
					try
					{
						info.Socket.Send(buffer.DataBuffer);
					}
					catch(Exception e)
					{
						info.IsDisconnected = true;
						throw;
					}
				}
			}
		}

		private static void SendMsg(NetBuff buffer, FrameData curFrameData)
		{
			NetSCSynchronizateMsg msg = new NetSCSynchronizateMsg();

			for(int i = 0; i < playerList.Count; i++)
			{
				var info = playerList[i];
				if(!info.IsDisconnected)
				{
					msg.FrameDatas.Clear();
					//补上差的帧
					for(int k = info.CurFrame + 1; k < curFrame; k++)
					{
						var f = historyFrameDatas.Find(o => o.Frame == k);
						if(f != null)
						{
							msg.FrameDatas.Add(f);
						}
					}
					msg.FrameDatas.Add(curFrameData);

					buffer.Reset();
					msg.Write(buffer);

					try
					{
						info.Socket.Send(buffer.DataBuffer);
					}
					catch(Exception e)
					{
						info.IsDisconnected = true;
						throw;
					}
				}
			}
		}
	}
}

