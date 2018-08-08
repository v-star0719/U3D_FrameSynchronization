using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostPlayer
{
	public static HostPlayer Instance = new HostPlayer();
	private const float MOUSE_SENSITIVITY = 50f;
	private const float FIRE_INVERVAL = 0.3f;

	public bool enableLookAround = false;

	public Player Player;
	private Camera playerCamera;

	private float preFireTime;


	//public void Init(Actor playerActor, Camera playerCamera)
	//{
	//	this.PlayerActor = playerActor;
	//	this.playerCamera = playerCamera;
	//}

	// Update is called once per frame
	public void Tick(float deltaTime)
	{
		if(Player == null)
		{
			return;
		}

		//视角偏转
		//float rotaionDeltaY = Input.GetAxis("Mouse X") * MOUSE_SENSITIVITY * Time.deltaTime;
		//float rotaionDeltaX = Input.GetAxis("Mouse Y") * MOUSE_SENSITIVITY * Time.deltaTime;
		float rotaionDeltaX = 0;
		float rotaionDeltaY = 0;

		if(enableLookAround)
			playerCamera.transform.localEulerAngles += new Vector3(rotaionDeltaX, 0, 0);
		//PlayerActor.Pitch(-rotaionDeltaX);

		//射击
		if(Input.GetAxis("Fire1") > 0 && Time.timeSinceLevelLoad - preFireTime >= FIRE_INVERVAL)
		{
			preFireTime = Time.timeSinceLevelLoad;
			//SynchronizationManager.Instance.SetOperation();
			return;
		}

		//扔手榴弹
		//if(Input.GetAxis("Fire2") > 0 && !PlayerActor.IsTossGrenade())
		//{
		//	preFireTime = Time.timeSinceLevelLoad;
		//	//PlayerActor.TossGrenade();
		//	return;
		//}

		float y = Input.GetAxis("Vertical");
		float x = Input.GetAxis("Horizontal");
		//跳
		//if(Input.GetAxis("Jump") > 0 && !PlayerActor.IsJumping())//
		//{
		//	//PlayerActor.Jump(x, 1, y);
		//	return;
		//}

		//前后
		if(y > 0)
		{
			SynchronizationManager.Instance.SetOperation(EmOperation.MOVE_FORWARD);
			return;
		}
		else if(y < 0)
		{
			SynchronizationManager.Instance.SetOperation(EmOperation.MOVE_BACKWARD);
			return;
		}

		//左后
		//如果没有前后移动，再去处理水平移动
		if(y != 0) x = 0;

		if(x > 0)
		{
			SynchronizationManager.Instance.SetOperation(EmOperation.MOVE_RIGHT);
			return;
		}
		else if(x < 0)
		{
			SynchronizationManager.Instance.SetOperation(EmOperation.MOVE_LEFT);
			return;
		}

		//转向
		//if(!PlayerActor.IsWalking() && !PlayerActor.IsJumping())
		//{
		//	if(rotaionDeltaY > 0)
		//	{
		//		PlayerActor.TrunRight(rotaionDeltaY);
		//	}
		//	else if(rotaionDeltaY < 0)
		//	{
		//		PlayerActor.TrunLeft(rotaionDeltaY);
		//	}
		//	return;
		//}

		//如果上面一个都没有，就待机
		if(!Player.PlayerActor.IsJumping() && !Player.PlayerActor.IsAttacking())
		{
			SynchronizationManager.Instance.SetOperation(EmOperation.IDLE);
		}
	}
}

