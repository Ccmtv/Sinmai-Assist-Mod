using System;
using HarmonyLib;
using Main;
using MelonLoader;
using UnityEngine;

namespace SinmaiAssist.Common;

public class DisableMask
{
	[HarmonyPrefix]
	[HarmonyPatch(typeof(GameMain), "LateInitialize", new Type[]
	{
		typeof(MonoBehaviour),
		typeof(Transform),
		typeof(Transform)
	})]
	public static bool LateInitialize(MonoBehaviour gameMainObject, ref Transform left, ref Transform right)
	{
		try
		{
			GameObject.Find("Mask").SetActive(value: false);
		}
		catch (Exception)
		{
			MelonLogger.Warning("Maybe the current Sinmai build does not have GameObject \"Mask\".");
		}
		return true;
	}
}
