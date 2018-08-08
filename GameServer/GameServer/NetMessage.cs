using System;
using System.Collections;
using System.Collections.Generic;

public enum EmNetMessageType
{
	SC_SELF_INFO,
	SC_PLAYER_LIST,
	SC_PLAYER_JOIN,
	SC_PLAYER_EXIT,
	SC_SYNCHRONIZATE_START,
	SC_SYNCHRONIZATE_END,
	SC_SYNCHRONIZATE,


	CS_CONNECT,
	CS_SYNCHRONIZATE,
}


public static class NetMessageFactory
{
	public static NetMessageBase GetMessage(int messageType)
	{
		EmNetMessageType type = (EmNetMessageType)messageType;
		switch(type)
		{
			case EmNetMessageType.SC_SELF_INFO:
				return new NetSCSelfInfoMsg();
			case EmNetMessageType.SC_PLAYER_LIST:
				return new NetSCPlayerListMsg();
			case EmNetMessageType.SC_PLAYER_JOIN:
				return new NetSCPlayerJoinMsg();
			case EmNetMessageType.SC_PLAYER_EXIT:
				return new NetSCPlayerExitMsg();
			case EmNetMessageType.SC_SYNCHRONIZATE_START:
				return new NetSCSynchronizateStartMsg();
			case EmNetMessageType.SC_SYNCHRONIZATE_END:
				return new NetSCSynchronizateEndMsg();
			case EmNetMessageType.SC_SYNCHRONIZATE:
				return new NetSCSynchronizateMsg();

			case EmNetMessageType.CS_SYNCHRONIZATE:
				return new NetCSSynchronizateMsg();
			case EmNetMessageType.CS_CONNECT:
				return new NetCSConnectMsg();
		}

		return null;
	}
}


public class NetMessageBase
{
	public EmNetMessageType MessageType;

	public virtual void Read(NetBuff buffer)
	{
		int n = buffer.ReadInt();
		MessageType = (EmNetMessageType)n;
	}

	public virtual void Write(NetBuff buffer)
	{
		int n = (int)MessageType;
		buffer.WriteInt(n);
	}
}

public class NetSCPlayerListMsg : NetMessageBase
{
	public NetSCPlayerList PlayerList = new NetSCPlayerList();


	public NetSCPlayerListMsg()
	{
		MessageType = EmNetMessageType.SC_PLAYER_LIST;
	}

	public override void Read(NetBuff buffer)
	{
		base.Read(buffer);
		PlayerList.Read(buffer);
	}

	public override void Write(NetBuff buffer)
	{
		base.Write(buffer);
		PlayerList.Write(buffer);
	}
}

public class NetSCPlayerList
{
	public List<int> PlayerList = new List<int>();

	public void Read(NetBuff buffer)
	{
		int n = buffer.ReadInt();
		for(int i = 0; i < n; i++)
		{
			PlayerList.Add(buffer.ReadInt());
		}
	}

	public void Write(NetBuff buffer)
	{
		buffer.WriteInt(PlayerList.Count);
		for(int i = 0; i < PlayerList.Count; i++)
		{
			buffer.WriteInt(PlayerList[i]);
		}
	}
}

public class NetSCSelfInfoMsg : NetMessageBase
{
	public int PlayerId;

	public NetSCSelfInfoMsg()
	{
		MessageType = EmNetMessageType.SC_SELF_INFO;
	}

	public override void Read(NetBuff buffer)
	{
		base.Read(buffer);
		PlayerId = buffer.ReadInt();
	}

	public override void Write(NetBuff buffer)
	{
		base.Write(buffer);
		buffer.WriteInt(PlayerId);
	}
}

public class NetSCPlayerJoinMsg : NetMessageBase
{
	public NetSCPlayerList PlayerList = new NetSCPlayerList();

	public NetSCPlayerJoinMsg()
	{
		MessageType = EmNetMessageType.SC_PLAYER_JOIN;
	}

	public override void Read(NetBuff buffer)
	{
		base.Read(buffer);
		PlayerList.Read(buffer);
	}

	public override void Write(NetBuff buffer)
	{
		base.Write(buffer);
		PlayerList.Write(buffer);
	}
}

public class NetSCPlayerExitMsg : NetMessageBase
{
	public NetSCPlayerList PlayerList = new NetSCPlayerList();

	public NetSCPlayerExitMsg()
	{
		MessageType = EmNetMessageType.SC_PLAYER_EXIT;
	}

	public override void Read(NetBuff buffer)
	{
		base.Read(buffer);
		PlayerList.Read(buffer);
	}

	public override void Write(NetBuff buffer)
	{
		base.Write(buffer);
		PlayerList.Write(buffer);
	}
}

public class NetSCSynchronizateStartMsg : NetMessageBase
{
	public NetSCSynchronizateStartMsg()
	{
		MessageType = EmNetMessageType.SC_SYNCHRONIZATE_START;
	}

	public override void Read(NetBuff buffer)
	{
		base.Read(buffer);
	}

	public override void Write(NetBuff buffer)
	{
		base.Write(buffer);
	}
}

public class NetSCSynchronizateEndMsg : NetMessageBase
{
	public NetSCSynchronizateEndMsg()
	{
		MessageType = EmNetMessageType.SC_SYNCHRONIZATE_END;
	}

	public override void Read(NetBuff buffer)
	{
		base.Read(buffer);
	}

	public override void Write(NetBuff buffer)
	{
		base.Write(buffer);
	}
}

public enum EmOperation
{
	NONE,
	IDLE,
	MOVE_LEFT,
	MOVE_RIGHT,
	MOVE_FORWARD,
	MOVE_BACKWARD
}

public class FrameData
{
	public int Frame;
	public List<OperationData> OperationDatas = new List<OperationData>();
}

public class OperationData
{
	public int PlayerId;

	public EmOperation Operation;
}

public class NetSCSynchronizateMsg : NetMessageBase
{
	public List<FrameData> FrameDatas = new List<FrameData>();

	public NetSCSynchronizateMsg()
	{
		MessageType = EmNetMessageType.SC_SYNCHRONIZATE;
	}

	public override void Read(NetBuff buffer)
	{
		base.Read(buffer);

		int frameCount = buffer.ReadInt();
		for(int i = 0; i < frameCount; i++)
		{
			FrameData frame = new FrameData();
			frame.Frame = buffer.ReadInt();
			FrameDatas.Add(frame);
			int operationCount = buffer.ReadInt();
			for(int j = 0; j < operationCount; j++)
			{
				OperationData operation = new OperationData();
				operation.PlayerId = buffer.ReadInt();
				operation.Operation = (EmOperation)buffer.ReadInt();
				frame.OperationDatas.Add(operation);
			}
		}
	}

	public override void Write(NetBuff buffer)
	{
		base.Write(buffer);

		buffer.WriteInt(FrameDatas.Count);
		for(int i = 0; i < FrameDatas.Count; i++)
		{
			var frame = FrameDatas[i];
			buffer.WriteInt(frame.Frame);
			buffer.WriteInt(frame.OperationDatas.Count);
			for(int j = 0; j < frame.OperationDatas.Count; j++)
			{
				buffer.WriteInt(frame.OperationDatas[j].PlayerId);
				buffer.WriteInt((int)frame.OperationDatas[j].Operation);
			}
		}
	}
}

///===================================================================

public class NetCSSynchronizateMsg : NetMessageBase
{
	public int PlayerId = 0;
	public int CurFrame = 0;
	public OperationData OperationData = new OperationData();

	public NetCSSynchronizateMsg()
	{
		MessageType = EmNetMessageType.CS_SYNCHRONIZATE;
	}

	public override void Read(NetBuff buffer)
	{
		base.Read(buffer);

		PlayerId = buffer.ReadInt();
		CurFrame = buffer.ReadInt();
		OperationData.PlayerId = buffer.ReadInt();
		OperationData.Operation = (EmOperation)buffer.ReadInt();
	}

	public override void Write(NetBuff buffer)
	{
		base.Write(buffer);

		buffer.WriteInt(PlayerId);
		buffer.WriteInt(CurFrame);
		buffer.WriteInt(OperationData.PlayerId);
		buffer.WriteInt((int)OperationData.Operation);
	}
}

public class NetCSConnectMsg : NetMessageBase
{
	public int PlayerId = 0;

	public NetCSConnectMsg()
	{
		MessageType = EmNetMessageType.CS_CONNECT;
	}

	public override void Read(NetBuff buffer)
	{
		base.Read(buffer);

		PlayerId = buffer.ReadInt();
	}

	public override void Write(NetBuff buffer)
	{
		base.Write(buffer);

		buffer.WriteInt(PlayerId);
	}
}