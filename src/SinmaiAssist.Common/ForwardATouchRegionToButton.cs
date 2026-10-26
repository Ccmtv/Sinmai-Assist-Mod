using HarmonyLib;
using Manager;
using Process;

namespace SinmaiAssist.Common;

public class ForwardATouchRegionToButton
{
	private static bool IsPlaying;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(GameProcess), "OnUpdate")]
	public static void OnUpdate(GameProcess __instance)
	{
		IsPlaying = new NotesManager().IsPlaying();
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Manager.InputManager), "GetButtonDown")]
	public static void GetButtonDown(ref bool __result, int monitorId, Manager.InputManager.ButtonSetting button)
	{
		if (!IsPlaying && !__result)
		{
			if (button.ToString().StartsWith("Button"))
			{
				__result = Manager.InputManager.GetTouchPanelAreaDown(monitorId, (Manager.InputManager.TouchPanelArea)button);
			}
			else if (button.ToString().Equals("Select"))
			{
				__result = Manager.InputManager.GetTouchPanelAreaLongPush(monitorId, Manager.InputManager.TouchPanelArea.C1, 500L) || Manager.InputManager.GetTouchPanelAreaLongPush(monitorId, Manager.InputManager.TouchPanelArea.C2, 500L);
			}
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Manager.InputManager), "GetButtonPush")]
	public static void GetButtonPush(ref bool __result, int monitorId, Manager.InputManager.ButtonSetting button)
	{
		if (!IsPlaying && !__result && button.ToString().StartsWith("Button"))
		{
			__result = Manager.InputManager.GetTouchPanelAreaPush(monitorId, (Manager.InputManager.TouchPanelArea)button);
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Manager.InputManager), "GetButtonLongPush")]
	public static void GetButtonLongPush(ref bool __result, int monitorId, Manager.InputManager.ButtonSetting button, long msec)
	{
		if (!IsPlaying && !__result && button.ToString().StartsWith("Button"))
		{
			__result = Manager.InputManager.GetTouchPanelAreaLongPush(monitorId, (Manager.InputManager.TouchPanelArea)button, msec);
		}
	}
}
