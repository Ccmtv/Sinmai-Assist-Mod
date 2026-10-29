using System;
using UnityEngine;

namespace SinmaiAssist.GUI;

public class MainGUI
{
    private enum Toolbar
    {
        AutoPlay,
        CustomAutoPlay,
        FastSkip,
        ChartController,
        DummyLogin,
        UserData,
        Graphic,
        Debug
    }

    private static readonly string[] TabNames = new string[8] { "自动演奏", "自定义", "快速跳过", "谱面控制", "虚拟登录", "用户数据", "画面", "调试" };

    private Rect _panel = new Rect(90f, 80f, 396f, 540f);

    private Vector2 _scroll;

    private Toolbar _toolbar;

    private bool _backspaceKeyDown;

    private bool _dragging;

    private Vector2 _dragOffset;

    public static readonly Style Style = new Style();

    public void OnGUI()
    {
        Style.EnsureInit();
        if (DebugInput.GetKey(SinmaiAssist.KeyBindConfig.SinmaiAssist.ShowUserPanel.KeyCode))
        {
            if (!_backspaceKeyDown)
            {
                SinmaiAssist.MainConfig.ModSetting.ShowPanel = !SinmaiAssist.MainConfig.ModSetting.ShowPanel;
            }
            _backspaceKeyDown = true;
        }
        else
        {
            _backspaceKeyDown = false;
        }
        if (SinmaiAssist.MainConfig.ModSetting.ShowPanel)
        {
            DrawPanel();
            SinmaiAssist.MainConfig.ModSetting.ShowPanel = true;
        }
    }

    private void DrawPanel()
    {
        Rect panel = _panel;
        panel.x = Mathf.Clamp(panel.x, 8f, Mathf.Max(8f, (float)Screen.width - panel.width - 8f));
        panel.y = Mathf.Clamp(panel.y, 8f, Mathf.Max(8f, (float)Screen.height - panel.height - 8f));
        _panel = panel;

        UnityEngine.GUI.Box(panel, GUIContent.none, Style.Window);

        Rect header = new Rect(panel.x, panel.y, panel.width, 62f);
        UnityEngine.GUI.Label(new Rect(header.x + 20f, header.y + 11f, 240f, 24f), "Sinmai-Assist", Style.HeaderTitle);
        UnityEngine.GUI.Label(new Rect(header.x + 20f, header.y + 35f, 230f, 14f), I18n.T("maimai DX 助手 · 1.0.0"), Style.HeaderSub);
        if (SinmaiAssist.MainConfig.ModSetting.SafeMode)
        {
            UnityEngine.GUI.Label(new Rect(panel.xMax - 118f, panel.y + 14f, 58f, 20f), I18n.T("安全模式"), Style.Badge);
        }
        if (UnityEngine.GUI.Button(new Rect(panel.xMax - 54f, panel.y + 13f, 38f, 22f), I18n.T("EN", "中"), Style.Badge))
        {
            I18n.EnglishMode = !I18n.EnglishMode;
            SinmaiAssist.MainConfig.ModSetting.English = I18n.EnglishMode;
            SinmaiAssist.SaveMainConfig();
        }
        UnityEngine.GUI.DrawTexture(new Rect(panel.x + 18f, panel.y + 62f, panel.width - 36f, 1f), Style.LineTex);

        HandleDrag(header);

        GUILayout.BeginArea(new Rect(panel.x + 18f, panel.y + 72f, panel.width - 36f, panel.height - 88f));
        DrawTabBar();
        GUILayout.Space(6f);
        _scroll = GUILayout.BeginScrollView(_scroll, false, false, GUIStyle.none, GUIStyle.none);
        DrawContent();
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void DrawTabBar()
    {
        GUILayout.BeginHorizontal();
        for (int i = 0; i < 4; i++)
        {
            if (I18n.Button(TabNames[i], (int)_toolbar == i ? Style.TabActive : Style.Tab, GUILayout.Height(30f)))
            {
                _toolbar = (Toolbar)i;
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        for (int j = 4; j < TabNames.Length; j++)
        {
            if (I18n.Button(TabNames[j], (int)_toolbar == j ? Style.TabActive : Style.Tab, GUILayout.Height(30f)))
            {
                _toolbar = (Toolbar)j;
            }
        }
        GUILayout.EndHorizontal();
    }

    private void DrawContent()
    {
        if (_toolbar == Toolbar.AutoPlay && SinmaiAssist.MainConfig.Cheat.AutoPlay)
        {
            AutoPlayPanel.OnGUI();
        }
        else if (_toolbar == Toolbar.CustomAutoPlay && SinmaiAssist.MainConfig.Cheat.AutoPlay)
        {
            CustomAutoPlayPanel.OnGUI();
        }
        else if (_toolbar == Toolbar.FastSkip && SinmaiAssist.MainConfig.Cheat.FastSkip)
        {
            FastSkipPanel.OnGUI();
        }
        else if (_toolbar == Toolbar.ChartController && SinmaiAssist.MainConfig.Cheat.ChartController)
        {
            ChartControllerPanel.OnGUI();
        }
        else if (_toolbar == Toolbar.DummyLogin && SinmaiAssist.MainConfig.Common.DummyLogin.Enable)
        {
            DummyLoginPanel.OnGUI();
        }
        else if (_toolbar == Toolbar.UserData)
        {
            UserDataPanel.OnGUI();
        }
        else if (_toolbar == Toolbar.Graphic)
        {
            GraphicPanel.OnGUI();
        }
        else if (_toolbar == Toolbar.Debug)
        {
            DebugPanel.OnGUI();
        }
        else
        {
            DisablePanel();
        }
    }

    private void DisablePanel()
    {
        GUILayout.Space(40f);
        I18n.Label("功能未启用", Style.Title, GUILayout.Height(30f));
        I18n.Label("请先在 Sinmai-Assist/Config.yml 中开启对应功能，然后重启游戏。", Style.DisablePanel, GUILayout.Height(60f));
    }

    private void HandleDrag(Rect header)
    {
        Event current = Event.current;
        if (current.type == EventType.MouseDown && current.button == 0 && header.Contains(current.mousePosition))
        {
            _dragging = true;
            _dragOffset = current.mousePosition - _panel.position;
            current.Use();
        }
        else if (_dragging && current.type == EventType.MouseDrag)
        {
            _panel.position = current.mousePosition - _dragOffset;
            current.Use();
        }
        else if (current.type == EventType.MouseUp)
        {
            _dragging = false;
        }
    }
}

