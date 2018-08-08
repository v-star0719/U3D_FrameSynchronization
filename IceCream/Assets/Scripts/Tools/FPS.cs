using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPS : MonoBehaviour
{
	public static FPS Instance;

	private int counter;

	private float time;

	private int fps;
	// Use this for initialization
	void Start ()
	{
		time = Time.time;
		Instance = this;
	}
	
	// Update is called once per frame
	public void Tick ()
	{
		counter++;
		if(Time.time - time >= 1)
		{
			fps = (int)(counter / (Time.time - time));
			time = Time.time;
			counter = 0;
		}
	}

	void OnGUI()
	{
		GUILayout.BeginHorizontal();

		GUILayout.Box(fps + "fps", GUILayout.ExpandWidth(false));

		var t = string.Format("{0}/{1}", SynchronizationManager.Instance.FrameCounter, SynchronizationManager.Instance.FrameCounter);
		GUILayout.Box(t, GUILayout.ExpandWidth(false));

		if(GUILayout.Button(GameMain.instance.IsPuased ? "继续" : "暂停", GUILayout.ExpandWidth(false)))
		{
			GameMain.instance.IsPuased = !GameMain.instance.IsPuased;
		}

		GUILayout.EndHorizontal();
	}
}
