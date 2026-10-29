using Manager;
using SinmaiAssist.Cheat;
using SinmaiAssist.Common;
using SinmaiAssist.Utils;
using UnityEngine;

namespace SinmaiAssist.GUI;

public class DebugPanel
{
    public static bool UnityLogger;

    public static void OnGUI()
    {
        I18n.Label("测试工具", MainGUI.Style.Title);
        if (I18n.Button("显示触摸区域", MainGUI.Style.Button, GUILayout.Height(30f)))
        {
            global::SinmaiAssist.Common.InputManager.TouchAreaDisplayButton = true;
        }
        if (I18n.Button("发送测试消息", MainGUI.Style.Button, GUILayout.Height(30f)))
        {
            GameMessageManager.SendMessage(0, "Hello World!\nMonitorId: 0");
            GameMessageManager.SendMessage(1, "Hello World!\nMonitorId: 1");
        }
        if (I18n.Button("保存 P1 设置到默认选项", MainGUI.Style.Button, GUILayout.Height(30f)))
        {
            ChangeDefaultOption.SaveOptionFile(0L);
        }
        if (I18n.Button("保存 P2 设置到默认选项", MainGUI.Style.Button, GUILayout.Height(30f)))
        {
            ChangeDefaultOption.SaveOptionFile(1L);
        }
        if (I18n.Button("↑ ↓ ↑ ↓", MainGUI.Style.Button, GUILayout.Height(30f)))
        {
            SoundManager.PlaySE(Mai2.Mai2Cue.Cue.SE_ENTRY_AIME_ERROR, 1);
        }
        if (I18n.Button("切换 Utage 模式", MainGUI.Style.Button, GUILayout.Height(30f)))
        {
            UnlockUtage.SwitchUtageMode();
        }
        I18n.Label("界面开关", MainGUI.Style.Title);
        if (I18n.Button("切换显示信息", MainGUI.Style.Button, GUILayout.Height(30f)))
        {
            SinmaiAssist.MainConfig.ModSetting.ShowInfo = !SinmaiAssist.MainConfig.ModSetting.ShowInfo;
        }
        if (I18n.Button("切换显示 FPS", MainGUI.Style.Button, GUILayout.Height(30f)))
        {
            SinmaiAssist.MainConfig.Common.ShowFPS = !SinmaiAssist.MainConfig.Common.ShowFPS;
        }
        UnityLogger = I18n.Toggle(UnityLogger, "记录 Unity 调试日志", MainGUI.Style.Toggle, GUILayout.Height(28f));
    }
}
