using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetManager
{
	public static NetManager Instance = new NetManager();


	public void OnMessageRecevie()
	{
		
	}

	public void SendMessage()
	{
		
	}

	public void Tick(float deltaTime)
	{
		var msgs = NetThread.Instance.Messages;
		while(msgs.Count > 0)
		{
			var msg = msgs.Dequeue();
			ProcessMessage(msg);
		}
	}

	public void ProcessMessage(NetMessageBase msg)
	{
		switch(msg.MessageType)
		{
			case EmNetMessageType.SC_SELF_INFO:
				OnReceivePlayerInfo(msg as NetSCSelfInfoMsg);
				break;

			case EmNetMessageType.SC_SYNCHRONIZATE:
				OnReceiveSyncData(msg as NetSCSynchronizateMsg);
				break;
				
			case EmNetMessageType.SC_PLAYER_LIST:
				OnReceivePlayerList(msg as NetSCPlayerListMsg);
				break;

			case EmNetMessageType.SC_SYNCHRONIZATE_START:
				OnReceiveSyncStart(msg as NetSCSynchronizateStartMsg);
				break;
		}
	}

	public void OnReceivePlayerInfo(NetSCSelfInfoMsg msg)
	{
		if(GameMain.instance.OfflineModel)
		{
			return;
		}
		PlayerManager.Instance.CreateHostPlayer(msg.PlayerId);
	}

	public void OnReceiveSyncData(NetSCSynchronizateMsg msg)
	{
		SynchronizationManager.Instance.AddFrameData(msg.FrameDatas);
	}

	public void OnReceivePlayerList(NetSCPlayerListMsg msg)
	{
		for(int i = 0; i < msg.PlayerList.PlayerList.Count; i++)
		{
			var id = msg.PlayerList.PlayerList[i];

			bool find = false;
			foreach(var player in PlayerManager.Instance.Players)
			{
				if(id == player.PlayerId)
				{
					find = true;
				}
			}

			if(!find)
			{
				PlayerManager.Instance.CreatePlayer(id);
			}
		}
	}

	public void OnReceiveSyncStart(NetSCSynchronizateStartMsg msg)
	{
		//GameMain.instance.EnableSync = true;
		//Random.InitState(10000);
	}
}
