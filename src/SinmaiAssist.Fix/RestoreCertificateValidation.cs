using System.Net;
using HarmonyLib;
using Net;

namespace SinmaiAssist.Fix;

public class RestoreCertificateValidation
{
	[HarmonyPostfix]
	[HarmonyPatch(typeof(NetHttpClient), "Create")]
	private static void OnNetHttpClientCreate()
	{
		ServicePointManager.ServerCertificateValidationCallback = null;
	}
}
