using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorAI : MonoBehaviour
{
	enum EmMyStatus
	{
		Idle,
		RandomMoveDirection,
		MoveToTarget,
		RandomMoveDirectionAfterAttack,
		AttackPrepair,
		NormalAttack,
		BeAttacked,
	}

	public Actor Actor;

	private EmMyStatus curStatus;
	private float statusDuration;
	private float timer;

	private Vector3 randomMoveDir;
	private Vector3 randomMovePos;

	private float lastAttackTime = 0;//上次攻击的时刻，限制连续两轮攻击的时间间隔

	private Actor attackTarget;
	private Actor moveToTarget;

	private float attackInterval = 1;
	private float moveMaxDuratoin = 5f;
	private float idlMaxDuration = 5f;
	private float patrolRadius = 30;
	private int idleRandom = 30;
	private int moveRandom = 70;
	private float attackPrepairDuration = 0.8f;

	public void StartAI(Actor actor)
	{
		this.Actor = actor;
		ChangeStatus(EmMyStatus.Idle);
	}

	// Update is called once per frame
	public void Tick(float deltaTime)
	{
		timer += deltaTime;

		if(Actor.IsBeAttacked())
		{
			ChangeStatus(EmMyStatus.BeAttacked);
			return;
		}

		if(Actor.IsDead() || Actor.IsDying())
		{
			return;
		}

		switch(curStatus)
		{
			case EmMyStatus.Idle:
				if(timer >= statusDuration)
				{
					int r = FixedRandom.Range(0, 101);
					if(r < idleRandom)
					{ }
					else if(r < idleRandom + moveRandom)
						ChangeStatus(EmMyStatus.RandomMoveDirection);
				}

				//if(Patrol()) MoveToTarget();

				break;
			case EmMyStatus.RandomMoveDirection:
			case EmMyStatus.RandomMoveDirectionAfterAttack:
				Actor.WalkForward(0);
				if(timer >= statusDuration)
					ChangeStatus(EmMyStatus.Idle);

				//if(curStatus == EmMyStatus.RandomMoveDirection)
				//	if(Patrol()) MoveToTarget();
				break;
			case EmMyStatus.MoveToTarget:
				Actor.LookAt(attackTarget.transform.position);
				Actor.WalkForward(0);
				if(Vector3.SqrMagnitude(Actor.transform.position - moveToTarget.transform.position) < 2 * 2)
				{
					ChangeStatus(EmMyStatus.AttackPrepair);
				}
				break;

			case EmMyStatus.AttackPrepair:
				if(timer >= attackPrepairDuration)
				{
					if(attackTarget != null && attackTarget.IsAlive() &&
					   (Actor.transform.position - moveToTarget.transform.position).sqrMagnitude <= 2 * 2)
						DoAttack();
					else
						ChangeStatus(EmMyStatus.Idle);
				}

				break;

			case EmMyStatus.NormalAttack:
				if(Actor.IsIdle())
					ChangeStatus(EmMyStatus.Idle);
				break;

			case EmMyStatus.BeAttacked:
				if(Actor.IsIdle())
					ChangeStatus(EmMyStatus.Idle);
				break;
		}
	}

	void ChangeStatus(EmMyStatus newStatus)
	{
		timer = 0f;
		curStatus = newStatus;
		switch(newStatus)
		{
			case EmMyStatus.Idle:
				Actor.Idle();
				statusDuration = FixedRandom.Range(0, idlMaxDuration);
				break;

			case EmMyStatus.RandomMoveDirection:
			case EmMyStatus.RandomMoveDirectionAfterAttack:
				Vector3[] dirs = new Vector3[]
				{
					new Vector3(0, 0, 1),
					new Vector3(1, 0, 1),
					new Vector3(1, 0, 0),
					new Vector3(1, 0, -1),
					new Vector3(0, 0, -1),
					new Vector3(-1, 0, -1),
					new Vector3(-1, 0, 0),
					new Vector3(-1, 0, 1),
				};
				randomMoveDir = dirs[FixedRandom.Range(0, 8)];
				randomMovePos = Actor.transform.position + randomMoveDir * 3;
				Actor.LookAt(randomMovePos);
				statusDuration = moveMaxDuratoin;
				break;

			case EmMyStatus.MoveToTarget:
				Actor.LookAt(attackTarget.transform.position);
				break;

			case EmMyStatus.AttackPrepair:
				Actor.Idle();
				statusDuration = attackPrepairDuration;
				break;

			case EmMyStatus.NormalAttack:
				Actor.Attack();
				break;

			case EmMyStatus.BeAttacked:
				break;
		}
	}

	void MoveToTarget()
	{
		//找最近的敌人，移动过去进行攻击
		Actor actor = GetNearestAttackTarget();
		if(actor != null)
		{
			attackTarget = actor;
			moveToTarget = actor;
			ChangeStatus(EmMyStatus.MoveToTarget);
		}
		else
			ChangeStatus(EmMyStatus.Idle);
	}

	void DoAttack()
	{
		ChangeStatus(EmMyStatus.NormalAttack);
	}


	//巡逻，发现敌人返回true，否则返回false
	bool Patrol()
	{
		if(Time.realtimeSinceStartup - lastAttackTime < attackInterval)
			return false;

		List<Actor> enemyList = GameMain.instance.actorList;
		for(int i = 0; i < enemyList.Count; i++)
		{
			if(!enemyList[i].IsAlive()) continue;
			float d = Vector3.SqrMagnitude(Actor.transform.position - enemyList[i].transform.position);
			if(d < patrolRadius * patrolRadius)
			{
				return true;
			}
		}
		return false;
	}

	Actor GetNearestAttackTarget()
	{
		List<Actor> enemyList = GameMain.instance.actorList;
		if(enemyList == null)
			return null;

		float minSqrDist = float.MaxValue;
		Actor minDistActor = null;
		for(int i = 0; i < enemyList.Count; i++)
		{
			if(!enemyList[i].IsAlive()) continue;
			float d = Vector3.SqrMagnitude(Actor.transform.position - enemyList[i].transform.position);
			if(d < minSqrDist)
			{
				minSqrDist = d;
				minDistActor = enemyList[i];
			}
		}
		return minDistActor;
	}
}
