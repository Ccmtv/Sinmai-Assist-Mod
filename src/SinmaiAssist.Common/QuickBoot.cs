using System;
using System.Reflection;
using HarmonyLib;
using Process;

namespace SinmaiAssist.Common;

public class QuickBoot
{
	[HarmonyPrefix]
	[HarmonyPatch(typeof(PowerOnProcess), "OnUpdate")]
	public static bool OnPowerOnUpdate(PowerOnProcess __instance)
	{
		AccessTools.Field(typeof(PowerOnProcess), "_waitTime").SetValue(__instance, 0.1f);
		FieldInfo fieldInfo = AccessTools.Field(typeof(PowerOnProcess), "_state");
		if (Convert.ToInt32(fieldInfo.GetValue(__instance)) == 2)
		{
			fieldInfo.SetValue(__instance, Convert.ToByte(3));
		}
		return true;
	}
}
