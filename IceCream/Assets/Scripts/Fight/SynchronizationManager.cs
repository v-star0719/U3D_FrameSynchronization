using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

//收到服务器的后开始同步
public class SynchronizationManager
{
	public static SynchronizationManager Instance = new SynchronizationManager();

	//初始就位于第一帧，理解为准备要跑的那一帧
	public int FrameCounter = 1;

	//接下来要跑的那一帧
	public int ServerFrameCounter = 1;

	public bool CouldRunCurrentFrame
	{
		get
		{
			return FrameCounter < ServerFrameCounter;
		}
	}


	private OperationData playerOperationData = new OperationData();
	private List<FrameData> frameDatas = new List<FrameData>();

	public void SetOperation(EmOperation operation)
	{
		playerOperationData.Operation = operation;
		playerOperationData.PlayerId = HostPlayer.Instance.Player.PlayerId;
	}

	public void AddFrameData(List<FrameData> datas)
	{
		//收到第一帧后开始
		GameMain.instance.EnableSync = true;

		//这里不严密，可能中间少帧
		foreach(var d in datas)
		{
			if(d.Frame == ServerFrameCounter)
			{
				frameDatas.Add(d);
				ServerFrameCounter++;
			}
		}

		//StringBuilder sb = new StringBuilder();
		//foreach(var d in frameDatas)
		//{
		//	sb.Append(d.Frame + " ");
		//}
		//Debug.Log("Receive frame " + sb);
		
		//if(frameDatas.Count == 0 || frameDatas[frameDatas.Count-1].Frame == datas[0].Frame)
		//{
		//	frameDatas.AddRange(datas);
		//	Debug.LogFormat("收到帧 {0}", datas.Count);
		//}
	}

	public void ClearOperation()
	{
		playerOperationData.Operation = EmOperation.NONE;
	}

	public void RunFrameStart()
	{
		//应用玩家操作
		if(frameDatas.Count > 0)
		{
			var frame = frameDatas[0];
			if(frame.Frame != FrameCounter)
			{
				Debug.LogErrorFormat("帧和服务器消息对不上 当前{0}, 服务器{1}", FrameCounter, frame.Frame);
			}
			else
			{
				foreach(var operation in frame.OperationDatas)
				{
					foreach(var player in PlayerManager.Instance.Players)
					{
						if(player.PlayerId == operation.PlayerId)
						{
							player.DoOperation(operation);
							break;
						}
					}
				}
			}
		}

		if(HostPlayer.Instance.Player != null)
		{
			HostPlayer.Instance.Player.DoOperation(playerOperationData);
		}

		ClearOperation();
	}

	public void RunFrameFinish()
	{		
		if(GameMain.instance.OfflineModel)
		{
			ServerFrameCounter++;
		}

		//处于最新一帧发送操作才是有效的
		//if(FrameCounter == ServerFrameCounter)
		{
			NetCSSynchronizateMsg msg = new NetCSSynchronizateMsg();
			msg.CurFrame = FrameCounter;
			msg.OperationData = playerOperationData;
			NetThread.Instance.Send(msg);
			//Debug.Log("发送 " + FrameCounter);	
		}
		
		//第一帧是没有数据的
		if(frameDatas.Count > 0)
		{
			frameDatas.RemoveAt(0);
		}

		FrameCounter++;
	}
}
