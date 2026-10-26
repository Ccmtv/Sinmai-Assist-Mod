using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Main;
using MelonLoader;
using Net;
using Net.Packet;
using Net.VO;

namespace SinmaiAssist.Fix;

public class DisableEncryption
{
	[HarmonyPatch]
	public class EncryptDecrypt
	{
		public static IEnumerable<MethodBase> TargetMethods()
		{
			MethodInfo[] methods = AccessTools.TypeByName("Net.CipherAES").GetMethods();
			return new _003C_003Ez__ReadOnlyArray<MethodBase>(new MethodBase[2]
			{
				methods.FirstOrDefault((MethodInfo it) => it.Name == "Encrypt" && it.IsPublic),
				methods.FirstOrDefault((MethodInfo it) => it.Name == "Decrypt" && it.IsPublic)
			});
		}

		public static bool Prefix(object[] __args, ref object __result)
		{
			if (!SinmaiAssist.MainConfig.Fix.DisableEncryption)
			{
				return true;
			}
			if (__args.Length == 1)
			{
				__result = __args[0];
			}
			else if (__args.Length == 2)
			{
				__args[1] = __args[0];
				__result = true;
			}
			return false;
		}
	}

	private static string apiSuffix = "";

	[HarmonyPostfix]
	[HarmonyPatch(typeof(GameMain), "LateInitialize")]
	public static void GetApiSuffix()
	{
		try
		{
			ConstructorInfo constructorInfo = typeof(NetQuery<VOSerializer, VOSerializer>).GetConstructors().First();
			apiSuffix = ((INetQuery)constructorInfo.Invoke(constructorInfo.GetParameters().Select((ParameterInfo parameter, int i) => (i != 0) ? parameter.DefaultValue : "").ToArray())).Api;
			MelonLogger.Msg("API suffix: " + apiSuffix);
		}
		catch (Exception arg)
		{
			MelonLogger.Error($"Failed to resolve the API suffix: {arg}");
			apiSuffix = null;
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Packet), "Obfuscator", new Type[] { typeof(string) })]
	public static bool PreObfuscator(string srcStr, ref string __result)
	{
		if (string.IsNullOrEmpty(apiSuffix))
		{
			return false;
		}
		if (srcStr.EndsWith(apiSuffix))
		{
			__result = srcStr.Substring(0, srcStr.Length - apiSuffix.Length);
		}
		return false;
	}
}
