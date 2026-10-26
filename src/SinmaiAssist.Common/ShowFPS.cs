using UnityEngine;

namespace SinmaiAssist.Common;

public class ShowFPS
{
	private static string fpsText = "FPS: ";

	private static float deltaTime = 0f;

	public static void OnGUI()
	{
		Update();
		GUIStyle gUIStyle = new GUIStyle();
		gUIStyle.fontSize = 24;
		gUIStyle.normal.textColor = Color.black;
		gUIStyle.alignment = TextAnchor.UpperLeft;
		UnityEngine.GUI.Label(new Rect(12f, 12f, 500f, 30f), fpsText, gUIStyle);
		gUIStyle.normal.textColor = Color.white;
		UnityEngine.GUI.Label(new Rect(10f, 10f, 500f, 30f), fpsText, gUIStyle);
	}

	private static void Update()
	{
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
		float f = 1f / deltaTime;
		fpsText = $"FPS: {Mathf.Ceil(f)}";
	}
}
