using System.Collections.Generic;
using SinmaiAssist.Cheat;
using SinmaiAssist.Config;
using UnityEngine;
using CustomAutoPlayConfig = SinmaiAssist.Config.MainConfig.CustomAutoPlayConfig;

namespace SinmaiAssist.GUI;

public class CustomAutoPlayPanel
{
    private static readonly Dictionary<string, string> FieldText = new Dictionary<string, string>();

    private static readonly Dictionary<string, int> FieldValue = new Dictionary<string, int>();

    public static void OnGUI()
    {
        I18n.Label("自定义 Autoplay", MainGUI.Style.Title);
        I18n.Label("严格数量模式：整首歌会恰好生成指定数量的非 Critical 判定，其余音符全部为 Critical Perfect。", MainGUI.Style.Muted);
        I18n.Label("注意：仅判定模式下，没打中的音符仍然是 Miss；方案只决定你打中后的判定。", MainGUI.Style.Muted);
        I18n.Label("数量超过谱面可抽取音符时会自动截断；全为 0 时整首歌为 Critical Perfect。", MainGUI.Style.Muted);
        I18n.Label("Slide（滑动）音符不会参与小P抽取；其它非 Critical 判定仍可落在 Slide 上。", MainGUI.Style.Muted);
        I18n.Label(AutoPlay.ClassicMode ? "当前行为：原版全自动（所有音符自动判定）" : "当前行为：仅判定（需要自己打，命中后才套用判定）", MainGUI.Style.Muted);
        I18n.Label("当前模式：" + ModeName(AutoPlay.autoPlayMode), MainGUI.Style.Text);

        bool lockMode = I18n.Toggle(AutoPlay.DisableUpdate, "锁定当前模式（不跟随游戏自动演奏状态切换）", MainGUI.Style.Toggle, GUILayout.Height(30f));
        if (lockMode != AutoPlay.DisableUpdate)
        {
            AutoPlay.DisableUpdate = lockMode;
        }
        GUILayout.Space(6f);

        GUILayout.BeginHorizontal();
        if (ModeButton("随机 All Perfect", AutoPlay.AutoPlayMode.RandomAllPerfect))
        {
            SelectMode(AutoPlay.AutoPlayMode.RandomAllPerfect);
        }
        if (ModeButton("随机 FC+", AutoPlay.AutoPlayMode.RandomFullComboPlus))
        {
            SelectMode(AutoPlay.AutoPlayMode.RandomFullComboPlus);
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (ModeButton("随机 FC", AutoPlay.AutoPlayMode.RandomFullCombo))
        {
            SelectMode(AutoPlay.AutoPlayMode.RandomFullCombo);
        }
        if (ModeButton("随机判定", AutoPlay.AutoPlayMode.Random))
        {
            SelectMode(AutoPlay.AutoPlayMode.Random);
        }
        GUILayout.EndHorizontal();
        if (I18n.Button("关闭自定义随机", MainGUI.Style.Button, GUILayout.Height(30f)))
        {
            AutoPlay.autoPlayMode = AutoPlay.AutoPlayMode.None;
        }
        GUILayout.Space(8f);

        CustomAutoPlayConfig config = GetConfig();
        if (config == null)
        {
            I18n.Label("配置尚未加载。", MainGUI.Style.ErrorMessage);
            return;
        }

        switch (AutoPlay.autoPlayMode)
        {
        case AutoPlay.AutoPlayMode.RandomAllPerfect:
            DrawAllPerfect(config);
            break;
        case AutoPlay.AutoPlayMode.RandomFullComboPlus:
            DrawFullComboPlus(config);
            break;
        case AutoPlay.AutoPlayMode.RandomFullCombo:
            DrawFullCombo(config);
            break;
        case AutoPlay.AutoPlayMode.Random:
            DrawRandom(config);
            break;
        default:
            I18n.Label("请选择一个随机模式，然后设置本曲需要的小P、Great、Good、Miss 数量。", MainGUI.Style.Muted);
            I18n.Label("提示：0 表示该判定不出现；未列出的音符全部是 Critical Perfect。", MainGUI.Style.Muted);
            break;
        }
    }

    private static void DrawAllPerfect(CustomAutoPlayConfig config)
    {
        I18n.Label("随机 All Perfect", MainGUI.Style.Title);
        int perfect = config.AllPerfectSmallPerfect;
        if (IntRow("ap_p", "小P（Perfect）", ref perfect))
        {
            config.AllPerfectSmallPerfect = perfect;
            SaveAndRefresh();
        }
        bool includeBreak = config.AllPerfectIncludeBreak;
        if (BreakRow("ap_break", "包含 Break（绝赞音符）", ref includeBreak))
        {
            config.AllPerfectIncludeBreak = includeBreak;
            SaveAndRefresh();
        }
        I18n.Label("其余音符：Critical Perfect。小P 只会是 Perfect，不会出现 Critical Perfect。", MainGUI.Style.Muted);
        I18n.Label(BreakHint(config.AllPerfectIncludeBreak), MainGUI.Style.Muted);
        DrawResetButton(config);
    }

    private static void DrawFullComboPlus(CustomAutoPlayConfig config)
    {
        I18n.Label("随机 FC+", MainGUI.Style.Title);
        int perfect = config.FullComboPlusSmallPerfect;
        if (IntRow("fcp_p", "小P（Perfect）", ref perfect))
        {
            config.FullComboPlusSmallPerfect = perfect;
            SaveAndRefresh();
        }
        int great = config.FullComboPlusGreat;
        if (IntRow("fcp_g", "Great 数量", ref great))
        {
            config.FullComboPlusGreat = great;
            SaveAndRefresh();
        }
        bool includeBreak = config.FullComboPlusIncludeBreak;
        if (BreakRow("fcp_break", "包含 Break（绝赞音符）", ref includeBreak))
        {
            config.FullComboPlusIncludeBreak = includeBreak;
            SaveAndRefresh();
        }
        I18n.Label("其余音符：Critical Perfect。", MainGUI.Style.Muted);
        I18n.Label(BreakHint(config.FullComboPlusIncludeBreak), MainGUI.Style.Muted);
        DrawResetButton(config);
    }

    private static void DrawFullCombo(CustomAutoPlayConfig config)
    {
        I18n.Label("随机 FC", MainGUI.Style.Title);
        int perfect = config.FullComboSmallPerfect;
        if (IntRow("fc_p", "小P（Perfect）", ref perfect))
        {
            config.FullComboSmallPerfect = perfect;
            SaveAndRefresh();
        }
        int great = config.FullComboGreat;
        if (IntRow("fc_g", "Great 数量", ref great))
        {
            config.FullComboGreat = great;
            SaveAndRefresh();
        }
        int good = config.FullComboGood;
        if (IntRow("fc_good", "Good 数量", ref good))
        {
            config.FullComboGood = good;
            SaveAndRefresh();
        }
        bool includeBreak = config.FullComboIncludeBreak;
        if (BreakRow("fc_break", "包含 Break（绝赞音符）", ref includeBreak))
        {
            config.FullComboIncludeBreak = includeBreak;
            SaveAndRefresh();
        }
        I18n.Label("其余音符：Critical Perfect。", MainGUI.Style.Muted);
        I18n.Label(BreakHint(config.FullComboIncludeBreak), MainGUI.Style.Muted);
        DrawResetButton(config);
    }

    private static void DrawRandom(CustomAutoPlayConfig config)
    {
        I18n.Label("随机判定", MainGUI.Style.Title);
        int perfect = config.RandomSmallPerfect;
        if (IntRow("rnd_p", "小P（Perfect）", ref perfect))
        {
            config.RandomSmallPerfect = perfect;
            SaveAndRefresh();
        }
        int great = config.RandomGreat;
        if (IntRow("rnd_g", "Great 数量", ref great))
        {
            config.RandomGreat = great;
            SaveAndRefresh();
        }
        int good = config.RandomGood;
        if (IntRow("rnd_good", "Good 数量", ref good))
        {
            config.RandomGood = good;
            SaveAndRefresh();
        }
        int miss = config.RandomMiss;
        if (IntRow("rnd_miss", "Miss 数量", ref miss))
        {
            config.RandomMiss = miss;
            SaveAndRefresh();
        }
        bool includeBreak = config.RandomIncludeBreak;
        if (BreakRow("rnd_break", "包含 Break（绝赞音符）", ref includeBreak))
        {
            config.RandomIncludeBreak = includeBreak;
            SaveAndRefresh();
        }
        I18n.Label("其余音符：Critical Perfect。Miss 会按未击中处理。", MainGUI.Style.Muted);
        I18n.Label(BreakHint(config.RandomIncludeBreak), MainGUI.Style.Muted);
        DrawResetButton(config);
    }

    private static void DrawResetButton(CustomAutoPlayConfig config)
    {
        GUILayout.Space(6f);
        if (I18n.Button("本模式数量归零", MainGUI.Style.Button, GUILayout.Height(30f)))
        {
            switch (AutoPlay.autoPlayMode)
            {
            case AutoPlay.AutoPlayMode.RandomAllPerfect:
                config.AllPerfectSmallPerfect = 0;
                config.AllPerfectIncludeBreak = false;
                break;
            case AutoPlay.AutoPlayMode.RandomFullComboPlus:
                config.FullComboPlusSmallPerfect = 0;
                config.FullComboPlusGreat = 0;
                config.FullComboPlusIncludeBreak = false;
                break;
            case AutoPlay.AutoPlayMode.RandomFullCombo:
                config.FullComboSmallPerfect = 0;
                config.FullComboGreat = 0;
                config.FullComboGood = 0;
                config.FullComboIncludeBreak = false;
                break;
            case AutoPlay.AutoPlayMode.Random:
                config.RandomSmallPerfect = 0;
                config.RandomGreat = 0;
                config.RandomGood = 0;
                config.RandomMiss = 0;
                config.RandomIncludeBreak = false;
                break;
            }
            SaveAndRefresh();
        }
    }

    private static string BreakHint(bool includeBreak)
    {
        return includeBreak
            ? "包含 Break：本模式的非 Critical 判定都可能落在绝赞音符上。"
            : "不包含 Break：绝赞音符固定为 Critical Perfect，非 Critical 判定只会在普通音符中抽取。";
    }

    private static bool ModeButton(string text, AutoPlay.AutoPlayMode mode)
    {
        bool active = AutoPlay.autoPlayMode == mode;
        return I18n.Button(text, active ? MainGUI.Style.TabActive : MainGUI.Style.Button, GUILayout.Height(32f));
    }

    private static void SelectMode(AutoPlay.AutoPlayMode mode)
    {
        AutoPlay.autoPlayMode = mode;
        AutoPlay.DisableUpdate = true;
    }

    private static bool IntRow(string key, string label, ref int value)
    {
        bool changed = false;
        GUILayout.BeginHorizontal();
        I18n.Label(label, MainGUI.Style.Text, GUILayout.Width(128f));
        if (I18n.Button("-", MainGUI.Style.Button, GUILayout.Width(30f), GUILayout.Height(26f)))
        {
            value = Mathf.Max(0, value - 1);
            changed = true;
        }

        if (!FieldText.TryGetValue(key, out string text))
        {
            text = value.ToString();
            FieldText[key] = text;
            FieldValue[key] = value;
        }
        else if (!FieldValue.TryGetValue(key, out int lastValue) || lastValue != value)
        {
            text = value.ToString();
            FieldText[key] = text;
            FieldValue[key] = value;
        }

        string input = GUILayout.TextField(text, MainGUI.Style.TextField, GUILayout.Width(60f), GUILayout.Height(26f));
        if (input != text)
        {
            FieldText[key] = input;
            if (int.TryParse(input, out int parsed))
            {
                parsed = Mathf.Clamp(parsed, 0, 9999);
                if (parsed != value)
                {
                    value = parsed;
                    changed = true;
                }
                FieldValue[key] = value;
                if (input != value.ToString())
                {
                    FieldText[key] = value.ToString();
                }
            }
        }

        if (I18n.Button("+", MainGUI.Style.Button, GUILayout.Width(30f), GUILayout.Height(26f)))
        {
            value = Mathf.Min(9999, value + 1);
            changed = true;
        }
        GUILayout.EndHorizontal();
        return changed;
    }

    private static bool BreakRow(string key, string label, ref bool value)
    {
        bool newValue = I18n.Toggle(value, label, MainGUI.Style.Toggle, GUILayout.Height(30f));
        if (newValue != value)
        {
            value = newValue;
            return true;
        }
        return false;
    }

    private static CustomAutoPlayConfig GetConfig()
    {
        if (SinmaiAssist.MainConfig == null || SinmaiAssist.MainConfig.Cheat == null)
        {
            return null;
        }
        if (SinmaiAssist.MainConfig.Cheat.CustomAutoPlay == null)
        {
            SinmaiAssist.MainConfig.Cheat.CustomAutoPlay = new CustomAutoPlayConfig();
        }
        return SinmaiAssist.MainConfig.Cheat.CustomAutoPlay;
    }

    private static void SaveAndRefresh()
    {
        SinmaiAssist.SaveMainConfig();
        AutoPlay.InvalidatePlans();
    }

    private static string ModeName(AutoPlay.AutoPlayMode mode)
    {
        return mode switch
        {
            AutoPlay.AutoPlayMode.RandomAllPerfect => "随机 All Perfect",
            AutoPlay.AutoPlayMode.RandomFullComboPlus => "随机 FC+",
            AutoPlay.AutoPlayMode.RandomFullCombo => "随机 FC",
            AutoPlay.AutoPlayMode.Random => "随机判定",
            AutoPlay.AutoPlayMode.Critical => "Critical（AP+）",
            AutoPlay.AutoPlayMode.Perfect => "Perfect",
            AutoPlay.AutoPlayMode.Great => "Great",
            AutoPlay.AutoPlayMode.Good => "Good",
            _ => "关闭"
        };
    }
}
