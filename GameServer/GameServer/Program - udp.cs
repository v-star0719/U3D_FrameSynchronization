//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Linq;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace GameServer
//{
//	class PlayerInfo
//	{
//		//public Socket Socket;
//		public int PlayerId;
//		public byte[] ReadBuffer = new byte[512];
//		public byte[] WriteBuffer = new byte[512];

//		public byte[] AnalysisBuffer = new byte[512];
//		public byte[] AnalysisTempBuffer = new byte[512];
//		public int AnalysisBufferLength = 0;

//		public bool IsDisconnected;

//		public OperationData OperationData;
//		public bool IsOperationReceived = false;

//		public NetBuff NetReadBuff = new NetBuff();
//		public NetBuff NetWriteBuff = new NetBuff();
//	}

//	class Program
//	{
//		private static Socket tcpSocket;

//		private static List<PlayerInfo> playerList = new List<PlayerInfo>();
//		private static int curPlayerId = 1;

//		private static NetBuff netReadBuff = new NetBuff();
//		private static NetBuff netWriteBuff = new NetBuff();

//		private static int curFrame = 1;
//		private static char inputChar;

//		private static bool isStart = false;

//		private static EndPoint targetEndPoint = new IPEndPoint(IPAddress.Any, 0);

//		public static byte[] readBuffer = new byte[1024];
//		public static byte[] writeBuffer = new byte[1024];

//		public static byte[] analysisBuffer = new byte[1024];
//		public static byte[] analysisTempBuffer = new byte[1024];
//		public static int analysisBufferLength = 0;

//		static void Main2(string[] args)
//		{
//			int recv;
//			byte[] revData = new byte[1024];
//			byte[] sendData = new byte[1024];
//			IPEndPoint ip = new IPEndPoint(IPAddress.Any, 6666);
//			Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
//			newsock.Bind(ip);
//			Console.WriteLine("我是服务端，主机名：{0}", Dns.GetHostName());
//			Console.WriteLine("等待客户端连接.....");
//			IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
//			EndPoint Remote = (EndPoint)(sender);
//			//recv = newsock.ReceiveFrom(revData, ref Remote);
//			Console.WriteLine("我是服务端，客户端{0}连接成功", Remote.ToString());
//			//Console.WriteLine(Encoding.Unicode.GetString(revData, 0, recv));
//			string welcome = "你好，我是服务器";
//			sendData = Encoding.Unicode.GetBytes(welcome);
//			newsock.SendTo(sendData, sendData.Length, SocketFlags.None, Remote);
//			while(true)
//			{
//				sendData = new byte[1024];
//				recv = newsock.ReceiveFrom(sendData, ref Remote);
//				string recvData = string.Format("客户端{0}发送：{1}", Remote.ToString(), Encoding.Unicode.GetString(sendData, 0, recv));
//				Console.WriteLine(recvData);
//				// string recvData =string.Format("服务器接收到数据{0}", Encoding.ASCII.GetString(data, 0, recv));
//				// byte.Parse(recvData);
//				string recvDateSucceed = string.Format("服务器已收到.");
//				sendData = Encoding.Unicode.GetBytes(recvDateSucceed);
//				newsock.SendTo(sendData, sendData.Length, SocketFlags.None, Remote);
//			}
//		}

//		//采用异步模式
//		static void Main(string[] args)
//		{
//			Thread t = new Thread(ServerCheckAliveThread);
//			t.Start();
//			t = new Thread(ServerSyncThread);
//			t.Start();


//			tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

//			//启动服务器
//			//IPAddress ip = IPAddress.Parse("127.0.0.1");
//			IPAddress ip = IPAddress.Any;
//			EndPoint endPoint = new IPEndPoint(ip, 6123);
//			tcpSocket.Bind(endPoint);
//			Console.WriteLine("服务器端启动完成，输入a开始同步");

//			//tcpSocket.SendTo(netWriteBuff.DataBuffer, targetEndPoint);
//			tcpSocket.BeginReceiveFrom(readBuffer, 0, readBuffer.Length, SocketFlags.None, ref targetEndPoint, OnReceive, null);

//			while(true)
//			{
//				var key = (char)Console.Read();
//				if(char.IsLetter(key))
//				{
//					inputChar = key;
//				}

//				Thread.Sleep(1000);
//			}

//			Console.ReadLine();
//		}

//		public static void OnConnected(int playerId)
//		{
//			//新建一个角色
//			PlayerInfo playerInfo = new PlayerInfo();
//			playerInfo.PlayerId = playerId;
//			playerList.Add(playerInfo);
//			Console.WriteLine("收到客户端连接，玩家id = " + playerInfo.PlayerId);

//			//玩家列表消息
//			NetSCPlayerListMsg msg2 = new NetSCPlayerListMsg();
//			for(int i = 0; i < playerList.Count; i++)
//			{
//				msg2.PlayerList.PlayerList.Add(playerList[i].PlayerId);
//			}
//			netWriteBuff.Reset();
//			msg2.Write(netWriteBuff);
//			tcpSocket.SendTo(netWriteBuff.DataBuffer, targetEndPoint);

//			//给所有玩家发送玩家列表
//			//for(int i = 0; i < playerList.Count; i++)
//			//{
//			//	try
//			//	{
//			//		playerList[i].Socket.Send(netWriteBuff.DataBuffer);
//			//	}
//			//	catch(Exception e)
//			//	{
//			//		playerList[i].IsDisconnected = true;
//			//	}
//			//}

//		}

//		public static void OnReceive(IAsyncResult ar)
//		{
//			int read = tcpSocket.EndReceiveFrom(ar, ref targetEndPoint);

//			if(read > 0)
//			{
//				Array.Copy(readBuffer, analysisBuffer, read);
//				analysisBufferLength += read;
//				AnalysisMsg();
//			}

//			try
//			{
//				tcpSocket.BeginReceiveFrom(readBuffer, 0, readBuffer.Length, SocketFlags.None, ref targetEndPoint, OnReceive, null);

//			}
//			catch(Exception e)
//			{
//			}
//		}

//		private static void AnalysisMsg()
//		{
//			while(analysisBufferLength > 0)
//			{
//				int msgLength = BitConverter.ToInt32(analysisBuffer, 0);
//				Console.WriteLine("msg length " + msgLength);
//				if(msgLength <= analysisBufferLength - 4)
//				{
//					netReadBuff.Set(analysisBuffer, 4, msgLength);
//					int messageType = BitConverter.ToInt32(analysisBuffer, 4);

//					var msg = NetMessageFactory.GetMessage(messageType);
//					if(msg != null)
//					{
//						msg.Read(netReadBuff);
//						if(messageType == (int)EmNetMessageType.CS_SYNCHRONIZATE)
//						{
//							var msgg = msg as NetCSSynchronizateMsg;
//							PlayerInfo info = playerList.Find(o => o.PlayerId == msgg.PlayerId);
//							if(info != null)
//							{
//								info.OperationData = msgg.OperationData;
//								info.IsOperationReceived = true;
//							}
//							else
//							{
//								Console.WriteLine("player is not exist");
//							}
//						}
//						else if(messageType == (int)EmNetMessageType.CS_CONNECT)
//						{
//							var msgg = msg as NetCSConnectMsg;
//							OnConnected(msgg.PlayerId);
//						}

//					}
//					else
//					{
//						Console.WriteLine("wrong message type " + messageType);
//					}

//					int remain = analysisBufferLength - msgLength - 4;
//					Array.Copy(analysisBuffer, 4 + msgLength, analysisTempBuffer, 0, remain);
//					Array.Copy(analysisTempBuffer, 0, analysisBuffer, 0, remain);
//					analysisBufferLength = remain;
//					//Debug.Log("remian " + remain);
//					//Log();
//				}
//				else
//				{
//					break;
//				}
//			}
//		}

//		static void ServerCheckAliveThread()
//		{
//			//使用心跳检查客户端是否存活
//			while(true)
//			{
//				for(int i = 0; i < playerList.Count; i++)
//				{
//					if(playerList[i].IsDisconnected)
//					{
//						Console.WriteLine("玩家 {0} 断线", playerList[i].PlayerId);
//						playerList.RemoveAt(i);
//						i--;
//					}
//				}

//				Thread.Sleep(10);
//			}
//		}

//		static void ServerSyncThread()
//		{
//			NetBuff writeBuffer = new NetBuff();

//			//使用心跳检查客户端是否存活
//			while(true)
//			{
//				//发送同步消息
//				if(!isStart)
//				{
//					if(inputChar == 'a' || inputChar == 'A')
//					{
//						NetSCSynchronizateStartMsg msg = new NetSCSynchronizateStartMsg();
//						SendMsg(writeBuffer, msg);
//						isStart = true;
//					}

//					Thread.Sleep(10);
//					continue;
//				}

//				bool isOk = true && playerList.Count > 0;
//				for(int i = 0; i < playerList.Count; i++)
//				{
//					if(!playerList[i].IsDisconnected && !playerList[i].IsOperationReceived)
//					{
//						isOk = false;
//					}
//				}
//				if(isOk)
//				{
//					//进入下一帧
//					curFrame++;
//					Console.WriteLine("start next frame " + curFrame);

//					//生成消息
//					NetSCSynchronizateMsg msg = new NetSCSynchronizateMsg();
//					var frameData = new FrameData();
//					msg.FrameDatas.Add(frameData);
//					frameData.Frame = curFrame;
//					for(int i = 0; i < playerList.Count; i++)
//					{
//						if(!playerList[i].IsDisconnected)
//						{
//							frameData.OperationDatas.Add(playerList[i].OperationData);
//							playerList[i].OperationData = null;
//							playerList[i].IsOperationReceived = false;
//						}
//					}
//					SendMsg(writeBuffer, msg);

//					Console.WriteLine("send msg finish");
//				}

//				Thread.Sleep(10);
//			}
//		}

//		private static void SendMsg(NetBuff buffer, NetMessageBase msg)
//		{
//			buffer.Reset();
//			msg.Write(buffer);

//			tcpSocket.SendTo(buffer.DataBuffer, targetEndPoint);

//			//for(int i = 0; i < playerList.Count; i++)
//			//{
//			//	if(!playerList[i].IsDisconnected)
//			//	{
//			//		try
//			//		{
//			//			playerList[i].Socket.Send(buffer.DataBuffer);
//			//		}
//			//		catch(Exception e)
//			//		{
//			//			playerList[i].IsDisconnected = true;
//			//			throw;
//			//		}
//			//	}
//			//}
//		}
//	}
//}

