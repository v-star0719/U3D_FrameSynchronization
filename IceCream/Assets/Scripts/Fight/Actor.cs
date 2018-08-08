using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public enum EmActorState
{
	Idle,
	WalkForward,
	WalkBackward,
	WalkLeft,
	WalkRight,
	Run,
	Jump,
	TurnLeft,
	TurnRight,
	Attack,
	TossGrenade,
	BeAttacked,
	Dying,
	Dead,
}

public enum EmActorType
{
	Player,
	Monster,
}

public struct CollisionInfo
{
	public Vector3 boxPos;
	public float dist;
	public Vector3 boxSize;

	public bool isClose
	{
		get
		{
			return dist - boxSize.x < 0.01f ||
			       dist - boxSize.z < 0.01f;
		}
	}
}

public class Actor : MonoBehaviour
{
	public const float GRAVITATIONAL_ACCELERATION = 10f;
	public const float JUMP_SPEED_XZ = 2;
	public const float JUMP_SPEED_Y = 3;

	public const float MOVE_SPEED = 2;

	public Vector3 lookRotation;
	public EmActorType actorType;

	private Rigidbody actorRigidbody;
	private Animation actorAnimation;
	private EmActorState myState = EmActorState.Idle;
	private float stateTimer;
	private float stateDuration;

	private bool hasDoAttack = false;
	public float hp;
	private GameObject weaponModel;

	private Vector3 jumpSpeed;

	private Transform collisionEnterBox;
	private float boxDist;

	public int ActorId;

	public void Init(int id)
	{
		if(actorAnimation != null) actorAnimation.gameObject.SetActive(true);

		actorAnimation = GetComponentInChildren<Animation>();
		actorRigidbody = GetComponentInChildren<Rigidbody>();
		ChangeState(EmActorState.Idle);
	}

	public void Idle()
	{
		ChangeState(EmActorState.Idle);
	}

	public void WalkForward(float deltaYAngle)
	{
		lookRotation.y += deltaYAngle;
		ChangeState(EmActorState.WalkForward);
	}

	public void WalkBackward(float deltaYAngle)
	{
		lookRotation.y += deltaYAngle;
		ChangeState(EmActorState.WalkBackward);
	}

	public void WalkLeft(float deltaYAngle)
	{
		lookRotation.y += deltaYAngle;
		ChangeState(EmActorState.WalkLeft);
	}

	public void WalkRight(float deltaYAngle)
	{
		lookRotation.y += deltaYAngle;
		ChangeState(EmActorState.WalkRight);
	}

	public void TrunLeft(float deltaYAngle)
	{
		lookRotation.y += deltaYAngle;
		ChangeState(EmActorState.TurnLeft);
	}

	public void TrunRight(float deltaYAngle)
	{
		lookRotation.y += deltaYAngle;
		ChangeState(EmActorState.TurnRight);
	}

	public void Pitch(float deltaXAngle)
	{
		lookRotation.x += deltaXAngle;
	}

	public void Jump(float x, float y, float z)
	{
		jumpSpeed = Vector3.zero;
		jumpSpeed.x = JUMP_SPEED_XZ * x;
		jumpSpeed.y = JUMP_SPEED_Y;
		jumpSpeed.z = JUMP_SPEED_XZ * z;

		Quaternion roate = transform.localRotation;
		jumpSpeed = roate * jumpSpeed;

		//jumpSpeed = Vector3.RotateTowards(jumpSpeed, transform.forward, Mathf.Deg2Rad*90, Mathf.Deg2Rad * 9090);
		//jumpSpeed = transform.worldToLocalMatrix.MultiplyVector(jumpSpeed);

		ChangeState(EmActorState.Jump);
	}

	public void Attack()
	{
		ChangeState(EmActorState.Attack);
	}

	public void TossGrenade()
	{
		ChangeState(EmActorState.TossGrenade);
	}

	public void LookAt(Vector3 target)
	{
		transform.LookAt(target);
		lookRotation = transform.localEulerAngles;
	}

	public bool IsIdle()
	{
		return myState == EmActorState.Idle;
	}

	public bool IsWalking()
	{
		return myState == EmActorState.WalkBackward || myState == EmActorState.WalkForward ||
		       myState == EmActorState.WalkLeft || myState == EmActorState.WalkRight;
	}

	public bool IsJumping()
	{
		return myState == EmActorState.Jump;
	}

	public bool IsAttacking()
	{
		return myState == EmActorState.Attack;
	}

	public bool IsBeAttacked()
	{
		return myState == EmActorState.BeAttacked;
	}

	public bool IsDead()
	{
		return myState == EmActorState.Dead;
	}

	public bool IsDying()
	{
		return myState == EmActorState.Dying;
	}

	public bool IsAlive()
	{
		return myState != EmActorState.Dying && myState != EmActorState.Dead;
	}

	public bool IsTossGrenade()
	{
		return myState == EmActorState.TossGrenade;
	}

	// Update is called once per frame
	public void Tick(float deltaTime)
	{
		stateTimer += deltaTime;
		switch(myState)
		{
			case EmActorState.Idle:
				break;

			case EmActorState.WalkForward:
				transform.localRotation = Quaternion.Euler(0, lookRotation.y, 0);
				SetActorPosWithCollision(transform.localPosition + transform.forward * MOVE_SPEED * deltaTime);
				break;

			case EmActorState.WalkBackward:
				transform.localRotation = Quaternion.Euler(0, lookRotation.y, 0);
				SetActorPosWithCollision(transform.localPosition - transform.forward * MOVE_SPEED * deltaTime);
				break;

			case EmActorState.WalkLeft:
				transform.localRotation = Quaternion.Euler(0, lookRotation.y, 0);
				SetActorPosWithCollision(transform.localPosition - transform.right * MOVE_SPEED * deltaTime);
				break;

			case EmActorState.WalkRight:
				transform.localRotation = Quaternion.Euler(0, lookRotation.y, 0);
				SetActorPosWithCollision(transform.localPosition + transform.right * MOVE_SPEED * deltaTime);
				break;

			case EmActorState.TurnLeft:
				//actorRigidbody.MoveRotation(Quaternion.Euler(0, lookRotation.y, 0));
				break;

			case EmActorState.TurnRight:
				//actorRigidbody.MoveRotation(Quaternion.Euler(0, lookRotation.y, 0));
				break;

			case EmActorState.Jump:
				jumpSpeed.y -= GRAVITATIONAL_ACCELERATION * deltaTime;
				Vector3 pos = transform.position + jumpSpeed * deltaTime;

				if(pos.y < 0 && jumpSpeed.y < 0)
				{
					pos.y = 0;
					transform.position = pos;
					ChangeState(EmActorState.Idle);
				}
				else
				{
					transform.position = pos;
				}
				break;

			case EmActorState.Attack:
				if(stateTimer >= stateDuration)
				{
					ChangeState(EmActorState.Idle);
				}
				break;

			case EmActorState.TossGrenade:
				if(stateTimer >= stateDuration)
				{
					ChangeState(EmActorState.Idle);
				}
				break;

			case EmActorState.BeAttacked:
				if(stateTimer >= stateDuration)
				{
					ChangeState(EmActorState.Idle);
				}
				break;

			case EmActorState.Dying:
				if(stateTimer >= stateDuration)
				{
					ChangeState(EmActorState.Dead);
				}
				break;
			case EmActorState.Dead:
				break;
		}

	}

	void ChangeState(EmActorState newState)
	{
		stateTimer = 0;
		myState = newState;
		switch(newState)
		{
			case EmActorState.Idle:
				//actorAnimation.Play(EmActorAnimaName.rifle_aiming_idle.ToString(), PlayMode.StopAll);
				break;
			case EmActorState.WalkForward:
				//actorAnimation.Play(EmActorAnimaName.walking.ToString(), PlayMode.StopAll);
				break;

			case EmActorState.WalkBackward:
				//actorAnimation.Play(EmActorAnimaName.walking_backwards.ToString(), PlayMode.StopAll);
				break;

			case EmActorState.WalkLeft:
				//actorAnimation.Play(EmActorAnimaName.strafe_left.ToString(), PlayMode.StopAll);
				break;

			case EmActorState.WalkRight:
				//actorAnimation.Play(EmActorAnimaName.strafe_right.ToString(), PlayMode.StopAll);
				break;

			case EmActorState.TurnLeft:
				//actorAnimation.Play(EmActorAnimaName.turn_left.ToString());
				break;

			case EmActorState.TurnRight:
				//actorAnimation.Play(EmActorAnimaName.turning_right_45_degrees.ToString());
				break;

			case EmActorState.Jump:
				//actorAnimation.Play(EmActorAnimaName.rifle_jump.ToString(), PlayMode.StopAll);
				break;

			case EmActorState.BeAttacked:
				//AnimationClip clip = actorAnimation.GetClip(EmActorAnimaName.hit_reaction.ToString());
				//if(clip != null)
				//{
				//	//actorAnimation.Play(EmActorAnimaName.hit_reaction.ToString(), PlayMode.StopAll);
				//	stateDuration = clip.length;
				//}
				//else
				//{
				//	stateDuration = 0.2f;
				//}
				break;

			case EmActorState.Attack:
				//actorAnimation.Play(EmActorAnimaName.firing_rifle.ToString(), PlayMode.StopAll);
				//stateDuration = actorAnimation.GetClip(EmActorAnimaName.firing_rifle.ToString()).length;
				//if(actorConf.attackTimePoint < 0.001f)//立即攻击
				//{
				//	DoAttack(actorConf.bulletId1);
				//	hasDoAttack = true;
				//}
				//else
				//	hasDoAttack = false;
				break;

			case EmActorState.TossGrenade:
				//actorAnimation.Play(EmActorAnimaName.toss_grenade.ToString(), PlayMode.StopAll);
				//stateDuration = actorAnimation.GetClip(EmActorAnimaName.toss_grenade.ToString()).length;
				//hasDoAttack = false;
				break;

			case EmActorState.Dying:
				//AnimationClip clip2 = actorAnimation.GetClip(EmActorAnimaName.dead.ToString());
				//if(clip2 != null)
				//{
				//	actorAnimation.Play(EmActorAnimaName.dead.ToString(), PlayMode.StopAll);
				//	stateDuration = clip2.length;
				//}
				//else
				//{
				//	stateDuration = 0.2f;
				//}
				break;
			case EmActorState.Dead:
				//if(actorType == EmActorType.Monster)
				//{
				//	gameObject.SetActive(false);
				//	GameMain.instance.OnEnemyDead();
				//}
				//else
				//{
				//	actorAnimation.gameObject.SetActive(false);
				//	GameMain.instance.OnPlayerDead();
				//}
				break;
		}
	}
	void DoAttack(int bulletId)
	{
		//BulletConf bulletConf = BulletDataCollection.GetConf(bulletId);
		//BulletBase b = BulletManager.GetBullet(bulletConf);

		//if(bulletConf.bulletType == EmBulletType.GunBullet)
		//	(b as GunBullet).Init(this, bulletConf, new Ray(actorBodyPoints.bulletOriginPos.position, actorBodyPoints.bulletDirPos.position - actorBodyPoints.bulletOriginPos.position));
		//else if(bulletConf.bulletType == EmBulletType.GrenadeBulle)
		//	(b as BulletGrenade).Init(this, bulletConf, actorBodyPoints.grenadeStartPos.position, actorBodyPoints.bulletDirPos.position - actorBodyPoints.bulletOriginPos.position);
		//else if(bulletConf.bulletType == EmBulletType.WorldPoint)
		//	(b as BulletWordPoint).Init(this, bulletConf, transform.position);
	}

	public void BeAttacked(float damage)
	{
		hp -= damage;
		if(hp > 0)
		{
			ChangeState(EmActorState.BeAttacked);
			//Debug.Log("hp = " + hp);
		}
		else
		{
			hp = 0;
			ChangeState(EmActorState.Dying);
		}

		//if(actorType == EmActorType.Player)
		//	UIManager.instance.mainUIPanel.SetPlayerHp((int)hp);
	}

	void OnDrawGizmos()
	{
		//if(Application.isPlaying && actorBodyPoints != null)
		//{
		//	Vector3 dir = actorBodyPoints.bulletDirPos.position - actorBodyPoints.bulletOriginPos.position;
		//	Gizmos.DrawRay(actorBodyPoints.bulletOriginPos.position, dir.normalized * 10);
		//}
	}

	private void DetectCollideWithBox(out CollisionInfo collisionInfo)
	{
		RaycastHit hitInfo;
		if(Physics.Raycast(transform.position, transform.forward, out hitInfo, 200))
		{
			collisionInfo.boxPos = hitInfo.transform.position;
			collisionInfo.boxPos.y = 0;
			collisionInfo.dist = Vector3.Distance(transform.position, collisionInfo.boxPos);
			collisionInfo.boxSize = hitInfo.collider.bounds.size;
		}
		else
		{
			collisionInfo.boxPos = Vector3.zero; ;
			collisionInfo.dist = float.MaxValue;
			collisionInfo.boxSize = Vector3.zero;
		}
	}

	private void SetActorPosWithCollision(Vector3 newPos)
	{
		if(newPos.x > GameMain.MAP_WIDTH * 0.5f)
			newPos.x = GameMain.MAP_WIDTH * 0.5f;
		if(newPos.x < -GameMain.MAP_WIDTH * 0.5f)
			newPos.x = -GameMain.MAP_WIDTH * 0.5f;
		if(newPos.z > GameMain.MAP_LENGTH * 0.5f)
			newPos.z = GameMain.MAP_LENGTH * 0.5f;
		if(newPos.z < -GameMain.MAP_LENGTH * 0.5f)
			newPos.z = -GameMain.MAP_LENGTH * 0.5f;

		transform.position = newPos;

		return;

		CollisionInfo collision1;
		DetectCollideWithBox(out collision1);
		if(collision1.isClose)
		{
			//距离变近，不能走
			if(collision1.dist > Vector3.Distance(newPos, collision1.boxPos))
				return;
		}
		transform.position = newPos;
	}

	//设置所有子节点的层级
	public static void SetAllChildLayer(Transform parent, int layer)
	{
		parent.gameObject.layer = layer;
		foreach(Transform trans in parent)
		{
			SetAllChildLayer(trans, layer);
		}
	}
}
