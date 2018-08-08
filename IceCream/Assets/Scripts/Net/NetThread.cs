using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class NetThread
{
	public static NetThread Instance = new NetThread();

	private Thread thread;
	private bool isStarted;
	private bool isStopped;
	private bool isConnected;

	private Socket tcpSocket;

	private byte[] receiveBuffer = new byte[1024];
	//private int receiveBufferDataLength;
	private byte[] sendBuffer = new byte[1024];

	private NetBuff readNetBuff = new NetBuff();
	private NetBuff writeNetBuff = new NetBuff();

	private byte[] analysisBuffer = new byte[1024];
	private byte[] analysisTempBuffer = new byte[1024];
	private int analysisBufferDataLength;

	public Queue<NetMessageBase> Messages = new Queue<NetMessageBase>();

	private bool isUDP = false;
	private EndPoint serverEndPoint;

	public void Start()
	{
		thread = new Thread(SocketThread);
		thread.Start();
		isStarted = true;
		isStopped = false;
		isConnected = false;
	}

	public void Stop()
	{
		isStopped = true;

		tcpSocket.Shutdown(SocketShutdown.Both);
	}

	public void SocketThread()
	{
		while(!isStopped)
		{
			if(!isConnected)
			{
				Connect();
			}
			Thread.Sleep(1000000);
		}
	}

	private void Connect()
	{
		if(isUDP)
		{
			Debug.Log("UDP");
			serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6123);
			tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			isConnected = true;

			NetCSConnectMsg msg = new NetCSConnectMsg();
			msg.PlayerId = GameMain.instance.playerId;
			Send(msg);
			tcpSocket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref serverEndPoint, OnReceve, null);
			Debug.Log("start receive");
		}
		else
		{
			Debug.Log("TCP");
			tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			tcpSocket.Connect(IPAddress.Parse("127.0.0.1"), 6123);
			isConnected = true;
			Debug.Log("connect successed");
			tcpSocket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, OnReceve, null);
		}
	}

	public void OnReceve(IAsyncResult ar)
	{
		int read = tcpSocket.EndReceive(ar);

		if(read > 0)
		{
			Array.Copy(receiveBuffer, 0, analysisBuffer, analysisBufferDataLength, read);
			analysisBufferDataLength += read;
			//Debug.LogFormat("receive {0}, now {1}", read, analysisBufferDataLength);
			//Log();
			AnalysisMsg();

			if(isUDP)
			{
				tcpSocket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref serverEndPoint, OnReceve, null);
			}
			else
			{
				tcpSocket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, OnReceve, null);
			}
		}
		else
		{
			Debug.LogFormat("receive end");
		}
	}

	public void Send(NetMessageBase netMessage)
	{
		writeNetBuff.Reset();
		netMessage.Write(writeNetBuff);

		if(isUDP)
		{
			tcpSocket.SendTo(writeNetBuff.DataBuffer, serverEndPoint);
		}
		else
		{
			SocketAsyncEventArgs args = new SocketAsyncEventArgs();
			var buffer = writeNetBuff.DataBuffer;
			args.SetBuffer(buffer, 0, buffer.Length);
			tcpSocket.SendAsync(args);
		}
	}

	private void AnalysisMsg()
	{
		while(analysisBufferDataLength > 0)
		{
			int msgLength = BitConverter.ToInt32(analysisBuffer, 0);
			//Debug.Log("msg length " + msgLength);
			if(msgLength <= analysisBufferDataLength - 4)
			{
				readNetBuff.Set(analysisBuffer, 4, msgLength);
				int messageType = BitConverter.ToInt32(analysisBuffer, 4);
				var msg = NetMessageFactory.GetMessage(messageType);
				if(msg != null)
				{
					msg.Read(readNetBuff);
					Messages.Enqueue(msg);
					//Debug.Log("receve " + msg.MessageType);
				}
				else
				{
					Debug.LogError("wrong message type " + messageType);
				}

				int remain = analysisBufferDataLength - msgLength - 4;
				Array.Copy(analysisBuffer, 4 + msgLength, analysisTempBuffer, 0, remain);
				Array.Copy(analysisTempBuffer, 0, analysisBuffer, 0, remain);
				analysisBufferDataLength = remain;
				//Debug.Log("remian " + remain);
				//Log();
			}
			else
			{
				break;
			}
		}
		
	}

	private void Log()
	{
		StringBuilder sb = new StringBuilder();
		for(int i = 0; i < analysisBufferDataLength; i++)
		{
			sb.AppendFormat("{0}({1}) ", i, analysisBuffer[i]);
		}
		Debug.Log(sb);
	}
}
