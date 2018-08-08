using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class GameMain : MonoBehaviour
{
	public const int MAX_ENEMY_COUNT = 20;
	public const int MIN_ENEMY_COUNT = 10;
	public const int MAP_WIDTH = 20;
	public const int MAP_LENGTH = 20;

	public static GameMain instance;

	public GameObject EnemyPrefab;
	public GameObject PlayerPrefab;
	
	public Camera PlayerCamera;
	public int playerId = 0;
	
	public List<Actor> actorList = new List<Actor>();
	public List<ActorAI> actorAIList = new List<ActorAI>();

	public Camera uiCamera;

	public int DebugFrameCount;
	public int DebugServerFrameCount;

	public bool OfflineModel = false;

	public bool EnableSync = false;

	public bool IsPuased = false;

	private float FrameTime;


	void Awake()
	{
		instance = this;
	}
	

	// Use this for initialization
	void Start()
	{
		//FixedRandom.DebugGetRandoms();

		Restart();

		int i = 0;
		foreach(var actor in actorList)
		{
			actor.Init(i++);
			var ai = actor.gameObject.AddComponent<ActorAI>();
			ai.StartAI(actor);
			actorAIList.Add(ai);
		}

		NetThread.Instance.Start();

		GameMain.instance.playerId = UnityEngine.Random.Range(0, 1000000);
		
		if(OfflineModel)
		{
			PlayerManager.Instance.CreateHostPlayer(playerId);
		}
	}

	void OnDestroy()
	{
		instance = null;
		NetThread.Instance.Stop();
	}

	// Update is called once per frame
	void Update()
	{
		NetManager.Instance.Tick(0);
		
		if(!EnableSync || IsPuased)
		{
			return;
		}

		float deltaTime = 0.025f;
		FrameTime += Time.deltaTime;
		if(!(FrameTime >= deltaTime))
		{
			return;
		}

		FrameTime -= deltaTime;

		DebugFrameCount = SynchronizationManager.Instance.FrameCounter;
		DebugServerFrameCount = SynchronizationManager.Instance.ServerFrameCounter;

		if(!SynchronizationManager.Instance.CouldRunCurrentFrame)
		{
			return;
		}

		if(FPS.Instance != null)
		{
			FPS.Instance.Tick();
		}

		SynchronizationManager.Instance.RunFrameStart();
		
		foreach(var actor in actorList)
		{
			actor.Tick(deltaTime);
		}

		foreach(var ai in actorAIList)
		{
			ai.Tick(deltaTime);
		}

		HostPlayer.Instance.Tick(deltaTime);

		SynchronizationManager.Instance.RunFrameFinish();
	}

	public void Restart()
	{
		//PlayerPrefab.transform.position = Vector3.zero;
		//ActorController.Init(PlayerActor, PlayerCamera);
		//ActorController.StartWork();
		//CreateEnemy();
	}

	void CreateEnemy()
	{
		int count = Random.Range(MIN_ENEMY_COUNT, MAX_ENEMY_COUNT);

		for(int i = 0; i < count; i++)
		{
			//Vector3 pos = Vector3.zero;
			//pos.x = Random.Range(0, MAP_WIDTH) - MAP_WIDTH * 0.5f;
			//pos.z = Random.Range(0, MAP_LENGTH) - MAP_LENGTH * 0.5f;

			//ActorBase actorBase = null;
			//EnemyAI ai = null;
			//if(i >= enemyList.Count)
			//{
			//	GameObject go = GameObject.Instantiate(EnemyPrefab);
			//	go.transform.position = pos;
			//	actorBase = go.AddComponent<ActorBase>();
			//	ai = go.AddComponent<EnemyAI>();
			//	enemyList.Add(actorBase);
			//}
			//else
			//{
			//	actorBase = enemyList[i];
			//	ai = enemyList[i].GetComponent<EnemyAI>();
			//}

			//actorBase.gameObject.SetActive(true);
			//actorBase.Init(2, EmActorType.Monster, -1);
			//actorBase.StartWork();
			//ai.StartAI(actorBase);
		}
	}

	void CreatePlayer()
	{
		
	}

	public void AddActor(Actor actor)
	{
		actorList.Add(actor);
	}

	public void OnEnemyDead()
	{

	}

	public void OnPlayerDead()
	{
	}

	public static bool IsInMap(Vector3 pos)
	{
		return Mathf.Abs(pos.x) > MAP_WIDTH * 0.5f || Mathf.Abs(pos.z) > MAP_LENGTH * 0.5f;
	}
}
