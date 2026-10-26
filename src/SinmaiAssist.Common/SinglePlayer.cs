using System;
using System.Reflection;
using HarmonyLib;
using Main;
using Manager;
using UnityEngine;

namespace SinmaiAssist.Common;

public class SinglePlayer
{
	[HarmonyPrefix]
	[HarmonyPatch(typeof(GameMainObject), "Start")]
	public static bool TurnOffRightMonitor(GameMainObject __instance)
	{
		Camera main = Camera.main;
		if (main == null)
		{
			return false;
		}
		Vector3 position = main.gameObject.transform.position;
		if (SinmaiAssist.MainConfig.Common.SinglePlayer.HideSubMonitor)
		{
			main.gameObject.transform.position = new Vector3(-540f, -420f, position.z);
			main.orthographicSize = 540f;
		}
		else
		{
			main.gameObject.transform.position = new Vector3(-540f, position.y, position.z);
		}
		((Transform)typeof(GameMainObject).GetField("rightMonitor", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance)).transform.localScale = Vector3.zero;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(MeshButton), "Awake")]
	public static bool FixDummyTouchPanel(MeshButton __instance)
	{
		CustomGraphic customGraphic = __instance.targetGraphic as CustomGraphic;
		FieldInfo fieldInfo = AccessTools.Field(typeof(MeshButton), "touchArea");
		FieldInfo fieldInfo2 = AccessTools.Field(typeof(MeshButton), "vertexArray");
		fieldInfo.SetValue(__instance, (Manager.InputManager.TouchPanelArea)Enum.Parse(typeof(Manager.InputManager.TouchPanelArea), __instance.name));
		Vector2[] array = new Vector2[customGraphic.vertex.Count];
		for (int i = 0; i < customGraphic.vertex.Count; i++)
		{
			if (SinmaiAssist.MainConfig.Common.SinglePlayer.HideSubMonitor)
			{
				array[i] = RectTransformUtility.WorldToScreenPoint(Camera.main, new Vector2((__instance.transform.position.x + customGraphic.vertex[i].x + 540f) * Camera.main.orthographicSize / 540f, (__instance.transform.position.y + customGraphic.vertex[i].y + 420f) * Camera.main.orthographicSize / 540f));
			}
			else
			{
				array[i] = RectTransformUtility.WorldToScreenPoint(Camera.main, new Vector2(__instance.transform.position.x + customGraphic.vertex[i].x + 540f, __instance.transform.position.y + customGraphic.vertex[i].y));
			}
		}
		fieldInfo2.SetValue(__instance, array);
		return false;
	}
}
