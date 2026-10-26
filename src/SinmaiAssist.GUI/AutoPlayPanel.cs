using SinmaiAssist.Cheat;
using UnityEngine;

namespace SinmaiAssist.GUI;

public class AutoPlayPanel
{
    public static void OnGUI()
    {
        GUILayout.Label("行为模式", MainGUI.Style.Title);
        bool classic = GUILayout.Toggle(AutoPlay.ClassicMode, "原版全自动（旧行为）", MainGUI.Style.Toggle, GUILayout.Height(30f));
        if (classic != AutoPlay.ClassicMode)
        {
            AutoPlay.ClassicMode = classic;
            SinmaiAssist.MainConfig.Cheat.ClassicAutoPlay = classic;
            SinmaiAssist.SaveMainConfig();
        }
        GUILayout.Label(AutoPlay.ClassicMode
            ? "当前：原版全自动 —— 自动打所有 note、自动滑星星，触摸会被屏蔽。"
            : "当前：仅判定模式 —— 自己打，命中后强制判定；星星要自己滑。", MainGUI.Style.Muted);
        GUILayout.Space(4f);

        GUILayout.Label("判定模式", MainGUI.Style.Title);
        GUILayout.Label("只修改判定结果，不会替你演奏。tap / hold / 星星都需要你自己打；touch 触摸圆点保持自动判定。", MainGUI.Style.Muted);
        GUILayout.Label("当前：" + ModeName(AutoPlay.autoPlayMode), MainGUI.Style.Text);
        AutoPlay.DisableUpdate = GUILayout.Toggle(AutoPlay.DisableUpdate, "锁定模式（不跟随游戏自动演奏状态切换）", MainGUI.Style.Toggle, GUILayout.Height(30f));
        GUILayout.Space(6f);

        if (ModeButton("Critical（AP+）", AutoPlay.AutoPlayMode.Critical, SinmaiAssist.KeyBindConfig.AutoPlay.Critical.KeyCode))
        {
            AutoPlay.autoPlayMode = AutoPlay.AutoPlayMode.Critical;
        }
        if (ModeButton("Perfect", AutoPlay.AutoPlayMode.Perfect, SinmaiAssist.KeyBindConfig.AutoPlay.Perfect.KeyCode))
        {
            AutoPlay.autoPlayMode = AutoPlay.AutoPlayMode.Perfect;
        }
        if (ModeButton("Great", AutoPlay.AutoPlayMode.Great, SinmaiAssist.KeyBindConfig.AutoPlay.Great.KeyCode))
        {
            AutoPlay.autoPlayMode = AutoPlay.AutoPlayMode.Great;
        }
        if (ModeButton("Good", AutoPlay.AutoPlayMode.Good, SinmaiAssist.KeyBindConfig.AutoPlay.Good.KeyCode))
        {
            AutoPlay.autoPlayMode = AutoPlay.AutoPlayMode.Good;
        }
        if (ModeButton("随机判定", AutoPlay.AutoPlayMode.Random, SinmaiAssist.KeyBindConfig.AutoPlay.Random.KeyCode))
        {
            AutoPlay.autoPlayMode = AutoPlay.AutoPlayMode.Random;
        }
        if (ModeButton("随机 All Perfect", AutoPlay.AutoPlayMode.RandomAllPerfect, SinmaiAssist.KeyBindConfig.AutoPlay.RandomAllPerfect.KeyCode))
        {
            AutoPlay.autoPlayMode = AutoPlay.AutoPlayMode.RandomAllPerfect;
        }
        if (ModeButton("随机 FC+", AutoPlay.AutoPlayMode.RandomFullComboPlus, SinmaiAssist.KeyBindConfig.AutoPlay.RandomFullComboPlus.KeyCode))
        {
            AutoPlay.autoPlayMode = AutoPlay.AutoPlayMode.RandomFullComboPlus;
        }
        if (ModeButton("随机 FC", AutoPlay.AutoPlayMode.RandomFullCombo, SinmaiAssist.KeyBindConfig.AutoPlay.RandomFullCombo.KeyCode))
        {
            AutoPlay.autoPlayMode = AutoPlay.AutoPlayMode.RandomFullCombo;
        }
        if (ModeButton("关闭", AutoPlay.AutoPlayMode.None, SinmaiAssist.KeyBindConfig.AutoPlay.None.KeyCode))
        {
            AutoPlay.autoPlayMode = AutoPlay.AutoPlayMode.None;
        }
    }

    private static bool ModeButton(string text, AutoPlay.AutoPlayMode mode, KeyCode keyCode)
    {
        bool active = AutoPlay.autoPlayMode == mode;
        return GUILayout.Button(text, active ? MainGUI.Style.TabActive : MainGUI.Style.Button, GUILayout.Height(32f))
            || DebugInput.GetKeyDown(keyCode);
    }

    private static string ModeName(AutoPlay.AutoPlayMode mode)
    {
        return mode switch
        {
            AutoPlay.AutoPlayMode.Critical => "Critical（AP+）",
            AutoPlay.AutoPlayMode.Perfect => "Perfect",
            AutoPlay.AutoPlayMode.Great => "Great",
            AutoPlay.AutoPlayMode.Good => "Good",
            AutoPlay.AutoPlayMode.Random => "随机判定",
            AutoPlay.AutoPlayMode.RandomAllPerfect => "随机 All Perfect",
            AutoPlay.AutoPlayMode.RandomFullComboPlus => "随机 FC+",
            AutoPlay.AutoPlayMode.RandomFullCombo => "随机 FC",
            _ => "关闭"
        };
    }
}
