using SinmaiAssist.Cheat;
using UnityEngine;

namespace SinmaiAssist.GUI;

public class FastSkipPanel
{
    private static string _scoreInput = "0";

    public static void OnGUI()
    {
        FastSkip.SkipButton = false;
        I18n.Label("跳过模式：" + (FastSkip.CustomSkip ? "自定义" : "默认"), MainGUI.Style.Text);
        I18n.Label("将当前乐曲直接结算为指定成绩。", MainGUI.Style.Muted);
        if (I18n.Button("立即跳过", new GUIStyle(MainGUI.Style.Button)
        {
            fontSize = 16
        }, GUILayout.Height(46f)))
        {
            FastSkip.SkipButton = true;
        }
        I18n.Label("模式设置", MainGUI.Style.Title);
        GUILayout.BeginHorizontal();
        if (I18n.Button("默认", FastSkip.CustomSkip ? MainGUI.Style.Button : MainGUI.Style.TabActive, GUILayout.Height(30f)))
        {
            FastSkip.CustomSkip = false;
        }
        if (I18n.Button("自定义", FastSkip.CustomSkip ? MainGUI.Style.TabActive : MainGUI.Style.Button, GUILayout.Height(30f)))
        {
            FastSkip.CustomSkip = true;
        }
        GUILayout.EndHorizontal();
        if (!FastSkip.CustomSkip)
        {
            return;
        }
        I18n.Label("自定义设置", MainGUI.Style.Title);
        I18n.Label("自定义分数（0 - 101）", MainGUI.Style.Text);
        _scoreInput = GUILayout.TextField(_scoreInput, MainGUI.Style.TextField, GUILayout.Height(28f));
        if (int.TryParse(_scoreInput, out var result))
        {
            if ((float)result >= 0f && (float)result <= 101f)
            {
                FastSkip.CustomAchivement = result;
                I18n.Label($"当前分数：{result} %", MainGUI.Style.Text);
            }
            else
            {
                I18n.Label("错误：请输入 0 到 101 之间的数值。", MainGUI.Style.ErrorMessage);
            }
        }
        else
        {
            I18n.Label("错误：请输入有效的整数。", MainGUI.Style.ErrorMessage);
        }
        FastSkip.Force1Miss = I18n.Toggle(FastSkip.Force1Miss, "强制附加 1 个 Miss", MainGUI.Style.Toggle, GUILayout.Height(28f));
    }
}
