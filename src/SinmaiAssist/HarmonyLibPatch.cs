using System.Collections;
using System.Reflection;
using HarmonyLib;
using SinmaiAssist.Attributes;

namespace SinmaiAssist;

public class HarmonyLibPatch
{
	[HarmonyPostfix]
	[HarmonyPatch("HarmonyLib.PatchTools", "GetPatchMethod")]
	public static void GetPatchMethod(ref MethodInfo __result)
	{
		if (__result != null)
		{
			EnableGameVersionAttribute customAttribute = __result.GetCustomAttribute<EnableGameVersionAttribute>();
			if (customAttribute != null && !customAttribute.ShouldEnable())
			{
				__result = null;
			}
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch("HarmonyLib.PatchTools", "GetPatchMethods")]
	public static void GetPatchMethods(ref IList __result)
	{
		for (int i = 0; i < __result.Count; i++)
		{
			EnableGameVersionAttribute customAttribute = (Traverse.Create(__result[i]).Field("info").GetValue() as HarmonyMethod).method.GetCustomAttribute<EnableGameVersionAttribute>();
			if (customAttribute != null && !customAttribute.ShouldEnable())
			{
				__result.RemoveAt(i);
				i--;
			}
		}
	}
}
