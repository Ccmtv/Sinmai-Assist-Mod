using System;
using HarmonyLib;
using Monitor;
using UnityEngine;

namespace SinmaiAssist.Common;

public class DisableBackground
{
	[HarmonyPrefix]
	[HarmonyPatch(typeof(MonitorBackgroundTownController), "SetMainDisp")]
	public static bool SetMainDisp(MonitorBackgroundTownController __instance, ref bool dispflag)
	{
		dispflag = false;
		DisableSubBackground();
		return true;
	}

	private static void DisableSubBackground()
	{
		try
		{
			if (!SinmaiAssist.MainConfig.Common.SinglePlayer.HideSubMonitor)
			{
				GameObject.Find("LeftMonitor").transform.Find("CommonProcess(Clone)").transform.Find("RearCanvas").transform.Find("Sub").Find("UI_SubBackground").gameObject.SetActive(value: false);
				GameObject.Find("RightMonitor").transform.Find("CommonProcess(Clone)").transform.Find("RearCanvas").transform.Find("Sub").Find("UI_SubBackground").gameObject.SetActive(value: false);
			}
		}
		catch (Exception)
		{
		}
	}
}
