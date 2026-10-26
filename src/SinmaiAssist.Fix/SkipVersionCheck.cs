using DB;
using HarmonyLib;
using MAI2System;
using Net.VO.Mai2;
using Process.Entry.State;
using SinmaiAssist.Utils;

namespace SinmaiAssist.Fix;

public class SkipVersionCheck
{
	[HarmonyPrefix]
	[HarmonyPatch(typeof(ConfirmPlay), "IsValidVersion")]
	public static bool IsValidVersion(ref bool __result, ref UserPreviewResponseVO vo)
	{
		VersionNo versionNo = default(VersionNo);
		VersionNo versionNo2 = default(VersionNo);
		versionNo.tryParse(vo.lastRomVersion, setZeroIfFailed: true);
		versionNo2.tryParse(vo.lastDataVersion, setZeroIfFailed: true);
		GameMessageManager.SendMessage(0, "RomVersion: " + versionNo.versionString + "\nDataVersion: " + versionNo2.versionString, "Account Version", WindowPositionID.Upper);
		__result = true;
		return false;
	}
}
