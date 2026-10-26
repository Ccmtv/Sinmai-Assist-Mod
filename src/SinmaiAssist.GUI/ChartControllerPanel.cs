using Manager;
using SinmaiAssist.Cheat;
using UnityEngine;

namespace SinmaiAssist.GUI;

public class ChartControllerPanel
{
    public static void OnGUI()
    {
        GUIStyle bigButton = new GUIStyle(MainGUI.Style.Button)
        {
            fontSize = 16
        };
        ChartController.ButtonStatus = ChartController.Button.None;
        NotesManager notesManager = new NotesManager();
        GUILayout.Label("计时器状态：" + ((!notesManager.IsPlaying()) ? "未播放" : (ChartController.IsPlaying ? "播放中" : "已暂停")), MainGUI.Style.Text);
        GUILayout.Label("计时", MainGUI.Style.Title);
        GUILayout.Label(ChartController.Timer.ToString("0000000.0000") ?? "", new GUIStyle(MainGUI.Style.Title)
        {
            fontSize = 22
        });
        if (GUILayout.Button(ChartController.IsPlaying ? "暂停" : "播放", bigButton, GUILayout.Height(44f)))
        {
            ChartController.ButtonStatus = ChartController.Button.Pause;
        }
        GUILayout.Space(4f);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("<<<", bigButton, GUILayout.Height(40f)))
        {
            ChartController.ButtonStatus = ChartController.Button.TimeSkipSub3;
        }
        if (GUILayout.Button("<<", bigButton, GUILayout.Height(40f)))
        {
            ChartController.ButtonStatus = ChartController.Button.TimeSkipSub2;
        }
        if (GUILayout.Button("<", bigButton, GUILayout.Height(40f)))
        {
            ChartController.ButtonStatus = ChartController.Button.TimeSkipSub;
        }
        if (GUILayout.Button(">", bigButton, GUILayout.Height(40f)))
        {
            ChartController.ButtonStatus = ChartController.Button.TimeSkipAdd;
        }
        if (GUILayout.Button(">>", bigButton, GUILayout.Height(40f)))
        {
            ChartController.ButtonStatus = ChartController.Button.TimeSkipAdd2;
        }
        if (GUILayout.Button(">>>", bigButton, GUILayout.Height(40f)))
        {
            ChartController.ButtonStatus = ChartController.Button.TimeSkipAdd3;
        }
        GUILayout.EndHorizontal();
        if (GUILayout.Button("重置", bigButton, GUILayout.Height(40f)))
        {
            ChartController.ButtonStatus = ChartController.Button.Reset;
        }
        GUILayout.Label(string.Format("记录时间：{0}（{1}）", ChartController.RecordTime.ToString("0000000"), -((int)ChartController.Timer - ChartController.RecordTime)), MainGUI.Style.Text);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("设置", MainGUI.Style.Button, GUILayout.Height(30f)))
        {
            ChartController.ButtonStatus = ChartController.Button.Set;
        }
        if (GUILayout.Button("返回", MainGUI.Style.Button, GUILayout.Height(30f)))
        {
            ChartController.ButtonStatus = ChartController.Button.Back;
        }
        GUILayout.EndHorizontal();
    }
}
