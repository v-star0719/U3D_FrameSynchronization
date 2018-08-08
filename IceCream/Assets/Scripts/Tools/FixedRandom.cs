using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class FixedRandom
{ 
	//public static FixedRandom Instance = new FixedRandom();

	private static float[] Randoms = new[]
	{
		0.028f, 0.774f, 0.317f, 0.412f, 0.347f, 0.581f, 0.744f, 0.395f, 0.221f, 0.135f,
		0.386f, 0.235f, 0.399f, 0.673f, 0.759f, 0.060f, 0.646f, 0.695f, 0.264f, 0.408f,
		0.473f, 0.172f, 0.200f, 0.621f, 0.013f, 0.341f, 0.654f, 0.715f, 0.390f, 0.840f,
		0.846f, 0.851f, 0.075f, 0.615f, 0.607f, 0.765f, 0.665f, 0.942f, 0.437f, 0.259f,
		0.216f, 0.308f, 0.978f, 0.293f, 0.688f, 0.370f, 0.849f, 0.287f, 0.522f, 0.277f,
		0.961f, 0.853f, 0.876f, 0.868f, 0.983f, 0.773f, 0.991f, 0.131f, 0.925f, 0.392f,
		0.801f, 0.793f, 0.725f, 0.336f, 0.895f, 0.781f, 0.392f, 0.352f, 0.498f, 0.397f,
		0.678f, 0.054f, 0.815f, 0.713f, 0.499f, 0.653f, 0.378f, 0.220f, 0.294f, 0.035f,
		0.470f, 0.647f, 0.884f, 0.123f, 0.691f, 0.698f, 0.571f, 0.594f, 0.931f, 0.574f,
		0.114f, 0.426f, 0.872f, 0.082f, 0.675f, 0.567f, 0.955f, 0.139f, 0.266f, 0.073f,
		0.051f, 0.829f, 0.060f, 0.152f, 0.348f, 0.636f, 0.584f, 0.609f, 0.595f, 0.701f,
		0.576f, 0.635f, 0.712f, 0.632f, 0.933f, 0.785f, 0.869f, 0.908f, 0.228f, 0.116f,
		0.322f, 0.717f, 0.514f, 0.019f, 0.936f, 0.651f, 0.904f, 0.329f, 0.998f, 0.703f,
		0.598f, 0.917f, 0.535f, 0.820f, 0.326f, 0.052f, 0.645f, 0.443f, 0.395f, 0.950f,
		0.687f, 0.846f, 0.950f, 0.630f, 0.571f, 0.359f, 0.875f, 0.931f, 0.927f, 0.713f,
		0.674f, 0.436f, 0.228f, 0.658f, 0.607f, 0.417f, 0.909f, 0.474f, 0.627f, 0.484f,
		0.907f, 0.370f, 0.122f, 0.017f, 0.396f, 0.271f, 0.610f, 0.718f, 0.792f, 0.614f,
		0.977f, 0.623f, 0.304f, 0.336f, 0.501f, 0.141f, 0.703f, 0.410f, 0.965f, 0.553f,
		0.942f, 0.284f, 0.897f, 0.532f, 0.126f, 0.177f, 0.803f, 0.798f, 0.875f, 0.982f,
		0.923f, 0.741f, 0.595f, 0.907f, 0.568f, 0.189f, 0.764f, 0.833f, 0.416f, 0.171f,
	};

	private static int randomIndex = 0;

	///生成一个0-1的数
	public static float Random()
	{
		float r = Randoms[randomIndex];
		randomIndex++;
		if(randomIndex >= Randoms.Length)
		{
			randomIndex = 0;
		}

		return r;
	}

	public static float Range(float min, float max)
	{
		float r = Random();
		return min + (max - min) * r;
	}

	public static int Range(int min, int max)
	{
		float r = Random();
		return min + (int)((max - min) * r);
	}

	public static  void DebugGetRandoms()
	{
		StringBuilder sb = new StringBuilder();
		for(int i = 0; i < 200; i++)
		{
			sb.AppendFormat("{0:f3}f, ", UnityEngine.Random.Range(0f, 1f));
			if(i % 10 == 9)
			{
				sb.Append("\n");
			}
		}
		Debug.Log(sb);
	}
}
