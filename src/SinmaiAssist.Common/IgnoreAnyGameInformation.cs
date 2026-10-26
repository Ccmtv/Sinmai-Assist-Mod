using System;
using HarmonyLib;
using MelonLoader;
using Net.Packet.Mai2;
using Net.VO.Mai2;

namespace SinmaiAssist.Common;

public class IgnoreAnyGameInformation
{
	[HarmonyPrefix]
	[HarmonyPatch(typeof(PacketGetUserData), "SafeNullMember")]
	public static bool SafeNullMember(PacketGetUserData __instance, UserDetailResponseVO src)
	{
		try
		{
			if (src.userData.eventWatchedDate != null && SinmaiAssist.MainConfig.Common.IgnoreAnyGameInformation)
			{
				src.userData.eventWatchedDate = "2099-12-31 23:59:59";
			}
			else if (src.userData.eventWatchedDate != null && DateTime.Parse(src.userData.eventWatchedDate) > DateTime.Now)
			{
				src.userData.eventWatchedDate = "2000-01-01 00:00:00";
			}
		}
		catch (Exception ex)
		{
			MelonLogger.Error($"Failed to patch: {ex}");
			MelonLogger.Error(ex.StackTrace);
		}
		return true;
	}
}
