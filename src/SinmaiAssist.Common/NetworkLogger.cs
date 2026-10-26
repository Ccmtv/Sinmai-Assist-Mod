using System;
using System.IO;
using HarmonyLib;
using MAI2.Util;
using Manager;
using MelonLoader;
using Net.Packet;
using Net.VO;

namespace SinmaiAssist.Common;

public class NetworkLogger
{
	private enum HttpMessageType
	{
		Request,
		Response,
		Null
	}

	private static readonly object logLock = new object();

	private static bool isInsideProcImpl = false;

	private static string NowApi = "";

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Packet), "set_State")]
	public static void RequestListener(Packet __instance)
	{
		if (__instance.State == PacketState.Ready)
		{
			string request = __instance.Query.GetRequest();
			NowApi = Singleton<OperationManager>.Instance.GetBaseUri() + __instance.Query.Api;
			PrintNetworkLog(HttpMessageType.Request, NowApi, request);
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(NetQuery<VOSerializer, VOSerializer>), "SetResponse")]
	public static void ResponseListener(ref string str)
	{
		if (isInsideProcImpl)
		{
			string content = str;
			PrintNetworkLog(HttpMessageType.Response, NowApi, content);
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Packet), "ProcImpl")]
	public static void ProcImplPrefix()
	{
		isInsideProcImpl = true;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Packet), "ProcImpl")]
	public static void ProcImplPostfix()
	{
		isInsideProcImpl = false;
	}

	private static void PrintNetworkLog(HttpMessageType httpMessageType, string api, string content)
	{
		if (httpMessageType == HttpMessageType.Null)
		{
			return;
		}
		try
		{
			DateTime now = DateTime.Now;
			string text = now.ToString("[HH:mm:ss.fff] ");
			text += $"[{httpMessageType}] [{api}] -> {content}\n";
			if (SinmaiAssist.MainConfig.Common.NetworkLogger.PrintToConsole)
			{
				MelonLogger.Msg($"[NetworkLogger] [{httpMessageType}] [{api}] -> {content}");
			}
			lock (logLock)
			{
				string text2 = Path.Combine("Sinmai-Assist/NetworkLogs");
				if (!Directory.Exists(text2))
				{
					Directory.CreateDirectory(text2);
				}
				File.AppendAllText(Path.Combine(text2, now.ToString("yyyy-MM-dd") + ".log"), text);
			}
		}
		catch (Exception obj)
		{
			MelonLogger.Error(obj);
		}
	}
}
