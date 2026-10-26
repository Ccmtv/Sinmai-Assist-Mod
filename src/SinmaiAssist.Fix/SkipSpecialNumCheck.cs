using HarmonyLib;
using Manager;

namespace SinmaiAssist.Fix;

public class SkipSpecialNumCheck
{
	[HarmonyPostfix]
	[HarmonyPatch(typeof(GameManager), "CalcSpecialNum")]
	private static void CalcSpecialNum(ref int __result)
	{
		int num = 0;
		num |= 0x400;
		__result = num;
	}
}
