using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	public Actor PlayerActor;
	public int PlayerId;

	public void DoOperation(OperationData operationData)
	{
		switch(operationData.Operation)
		{
			case EmOperation.IDLE:
				Idle();break;
			case  EmOperation.MOVE_FORWARD:
				WalkForward();break;
				case EmOperation.MOVE_BACKWARD:
				WalkBackward();break;
			case EmOperation.MOVE_LEFT:
				WalkLeft();break;
			case EmOperation.MOVE_RIGHT:
				WalkRight();break;
		}
	}

	public void Idle()
	{
		PlayerActor.Idle();
	}

	public void WalkForward()
	{
		PlayerActor.WalkForward(0);
	}

	public void WalkBackward()
	{
		PlayerActor.WalkBackward(0);
	}

	public void WalkLeft()
	{
		PlayerActor.WalkLeft(0);
	}

	public void WalkRight()
	{
		PlayerActor.WalkRight(0);
	}
}
