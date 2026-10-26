using HarmonyLib;
using Net;

namespace SinmaiAssist.Fix;

public class SkipCakeHashCheck
{
	[HarmonyPostfix]
	[HarmonyPatch(typeof(NetHttpClient), MethodType.Constructor)]
	private static void OnNetHttpClientConstructor(NetHttpClient __instance)
	{
		Traverse traverse = Traverse.Create(__instance).Field("isTrueDll");
		if (traverse.FieldExists())
		{
			traverse.SetValue(true);
		}
	}
}
