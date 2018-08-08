using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager
{
	public static PlayerManager Instance = new PlayerManager();

	public List<Player> Players = new List<Player>();

	public void SendOperationToServer()
	{
		
	}

	public void OnReceiveOperation()
	{
		
	}

	public void CreateHostPlayer(int playerId)
	{
		var p = CreatePlayer(playerId);
		p.gameObject.name = "Hostplayer" + playerId;
		HostPlayer.Instance.Player = p;
	}

	public Player CreatePlayer(int playerId)
	{
		GameObject go = GameObject.Instantiate(GameMain.instance.PlayerPrefab);
		go.transform.localPosition = new Vector3((playerId % 4 - 4) * 2, 0, 2);
		go.transform.localScale = Vector3.one;
		go.transform.localRotation = Quaternion.identity;
		go.name = "player" + playerId;

		var actor = go.AddComponent<Actor>();
		Player player = go.AddComponent<Player>();
		player.PlayerActor = actor;
		player.PlayerId = playerId;
		GameMain.instance.AddActor(actor);

		Players.Add(player);

		return player;
	}
}
