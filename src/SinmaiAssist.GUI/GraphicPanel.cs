using SinmaiAssist.Common;
using UnityEngine;

namespace SinmaiAssist.GUI;

public class GraphicPanel
{
    private static string screenWidth = $"{Graphic.GetResolutionWidth()}";

    private static string screenHeight = $"{Graphic.GetResolutionHeight()}";

    private static string frameRate = $"{Graphic.GetMaxFrameRate()}";

    public static void OnGUI()
    {
        if (GUILayout.Button("切换全屏 / 窗口", MainGUI.Style.Button, GUILayout.Height(42f)))
        {
            Graphic.ToggleFullscreen();
        }
        GUILayout.Label("自定义画面", MainGUI.Style.Title);
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        GUILayout.Label("宽度", MainGUI.Style.Text);
        screenWidth = GUILayout.TextField(screenWidth, MainGUI.Style.TextField, GUILayout.Height(28f));
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        GUILayout.Label("高度", MainGUI.Style.Text);
        screenHeight = GUILayout.TextField(screenHeight, MainGUI.Style.TextField, GUILayout.Height(28f));
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.Label("最大帧率（-1 为不限制）", MainGUI.Style.Text);
        frameRate = GUILayout.TextField(frameRate, MainGUI.Style.TextField, GUILayout.Height(28f));
        if (GUILayout.Button("应用", MainGUI.Style.Button, GUILayout.Height(34f)) && int.TryParse(screenWidth, out var result) && int.TryParse(screenHeight, out var result2) && int.TryParse(frameRate, out var result3) && (float)result >= 360f && (float)result2 >= 360f)
        {
            Graphic.SetResolution(result, result2);
            Graphic.SetMaxFrameRate(result3);
        }
    }
}
