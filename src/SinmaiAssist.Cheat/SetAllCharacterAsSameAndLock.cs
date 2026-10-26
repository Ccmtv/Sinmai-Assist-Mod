using HarmonyLib;
using Net.Packet.Mai2;
using Net.VO.Mai2;

namespace SinmaiAssist.Cheat;

public class SetAllCharacterAsSameAndLock
{
	[HarmonyPrefix]
	[HarmonyPatch(typeof(PacketGetUserData), "SafeNullMember")]
	public static bool SafeNullMember(PacketGetUserData __instance, UserDetailResponseVO src)
	{
		if (src.userData.charaSlot != null)
		{
			int[] array = new int[5]
			{
				src.userData.charaSlot[0],
				src.userData.charaSlot[0],
				src.userData.charaSlot[0],
				src.userData.charaSlot[0],
				src.userData.charaSlot[0]
			};
			src.userData.charaSlot = array;
			src.userData.charaLockSlot = array;
		}
		return true;
	}
}
