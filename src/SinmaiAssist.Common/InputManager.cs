using System.Reflection;
using HarmonyLib;
using Manager;

namespace SinmaiAssist.Common;

public class InputManager
{
	public static bool TouchAreaDisplayButton;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Manager.InputManager), "UpdateAmInput")]
	public static void TouchAreaDisplay()
	{
		if (TouchAreaDisplayButton)
		{
			FieldInfo field = typeof(Manager.InputManager).GetField("_isMouseTouchPanelVisible", BindingFlags.Static | BindingFlags.NonPublic);
			bool flag = !(bool)field.GetValue(typeof(Manager.InputManager));
			field.SetValue(null, flag);
			MouseTouchPanel[] mouseTouchPanel = Manager.InputManager.MouseTouchPanel;
			for (int i = 0; i < mouseTouchPanel.Length; i++)
			{
				mouseTouchPanel[i].SetVisible(flag);
			}
			TouchAreaDisplayButton = false;
		}
	}
}
