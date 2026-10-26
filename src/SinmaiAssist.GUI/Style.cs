using UnityEngine;

namespace SinmaiAssist.GUI;

public class Style
{
    public static readonly Color ColorBg = new Color(0.055f, 0.055f, 0.085f, 0.97f);
    public static readonly Color ColorCard = new Color(1f, 1f, 1f, 0.045f);
    public static readonly Color ColorPink = new Color(1f, 0.30f, 0.55f, 1f);
    public static readonly Color ColorCyan = new Color(0.25f, 0.88f, 1f, 1f);
    public static readonly Color ColorText = new Color(0.94f, 0.94f, 0.97f, 1f);
    public static readonly Color ColorMuted = new Color(0.58f, 0.58f, 0.67f, 1f);
    public static readonly Color ColorDanger = new Color(1f, 0.36f, 0.47f, 1f);

    public Font Font;

    public GUIStyle Window;
    public GUIStyle Card;
    public GUIStyle HeaderTitle;
    public GUIStyle HeaderSub;
    public GUIStyle Badge;
    public GUIStyle Title;
    public GUIStyle Text;
    public GUIStyle Muted;
    public GUIStyle Button;
    public GUIStyle Tab;
    public GUIStyle TabActive;
    public GUIStyle Toggle;
    public GUIStyle TextField;
    public GUIStyle ErrorMessage;
    public GUIStyle DisablePanel;

    public Texture2D LineTex;

    private bool _ready;

    public void EnsureInit()
    {
        if (_ready)
        {
            return;
        }
        _ready = true;
        Font = LoadFont(16);
        Build();
    }

    private static Font LoadFont(int size)
    {
        string[] candidates = new string[6]
        {
            "Microsoft YaHei UI",
            "Microsoft YaHei",
            "SimHei",
            "Noto Sans CJK SC",
            "Source Han Sans SC",
            "Arial Unicode MS"
        };
        try
        {
            Font font = Font.CreateDynamicFontFromOSFont(candidates, size);
            if (font != null)
            {
                return font;
            }
        }
        catch
        {
        }
        try
        {
            return Font.CreateDynamicFontFromOSFont("Arial", size);
        }
        catch
        {
            return null;
        }
    }

    private static Color C(float r, float g, float b, float a = 1f)
    {
        return new Color(r, g, b, a);
    }

    private void Build()
    {
        Window = Box(Round(18, ColorBg), 20);
        Window.padding = new RectOffset(0, 0, 0, 0);

        Card = Box(Round(10, ColorCard), 12);
        Card.padding = new RectOffset(12, 12, 10, 10);

        HeaderTitle = Label(18, ColorText, TextAnchor.MiddleLeft);
        HeaderTitle.fontStyle = FontStyle.Bold;

        HeaderSub = Label(10, ColorMuted, TextAnchor.MiddleLeft);

        Badge = Label(10, new Color(0.08f, 0.05f, 0.08f), TextAnchor.MiddleCenter);
        Badge.normal.background = Round(8, ColorPink);
        Badge.border = new RectOffset(8, 8, 8, 8);
        Badge.padding = new RectOffset(8, 8, 2, 2);
        Badge.fontStyle = FontStyle.Bold;

        Title = Label(13, ColorText, TextAnchor.MiddleLeft);
        Title.fontStyle = FontStyle.Bold;
        Title.margin = new RectOffset(2, 2, 12, 4);

        Text = Label(12, ColorText, TextAnchor.MiddleLeft);
        Text.margin = new RectOffset(2, 2, 2, 2);

        Muted = Label(11, ColorMuted, TextAnchor.UpperLeft);
        Muted.wordWrap = true;
        Muted.margin = new RectOffset(2, 2, 2, 8);

        Button = Box(Round(9, C(0.145f, 0.145f, 0.195f)), 10);
        Button.hover.background = Round(9, C(0.20f, 0.20f, 0.28f));
        Button.active.background = Round(9, C(0.26f, 0.26f, 0.37f));
        Button.onNormal.background = Round(9, ColorPink);
        Button.fontSize = 12;
        Button.alignment = TextAnchor.MiddleCenter;
        Button.padding = new RectOffset(10, 10, 6, 6);
        Button.margin = new RectOffset(0, 0, 4, 4);
        Button.normal.textColor = ColorText;
        Button.hover.textColor = Color.white;
        Button.active.textColor = Color.white;
        Button.onNormal.textColor = new Color(0.09f, 0.04f, 0.08f);

        Tab = Box(Round(9, C(0.115f, 0.115f, 0.155f, 0.85f)), 10);
        Tab.hover.background = Round(9, C(0.17f, 0.17f, 0.24f));
        Tab.active.background = Round(9, C(0.20f, 0.20f, 0.28f));
        Tab.fontSize = 12;
        Tab.alignment = TextAnchor.MiddleCenter;
        Tab.padding = new RectOffset(6, 6, 6, 6);
        Tab.margin = new RectOffset(2, 2, 2, 2);
        Tab.normal.textColor = ColorMuted;
        Tab.hover.textColor = ColorText;

        TabActive = Box(Round(9, ColorPink), 10);
        TabActive.fontSize = 12;
        TabActive.alignment = TextAnchor.MiddleCenter;
        TabActive.padding = new RectOffset(6, 6, 6, 6);
        TabActive.margin = new RectOffset(2, 2, 2, 2);
        TabActive.normal.textColor = new Color(0.09f, 0.04f, 0.08f);
        TabActive.hover.textColor = new Color(0.09f, 0.04f, 0.08f);
        TabActive.active.textColor = new Color(0.09f, 0.04f, 0.08f);

        Toggle = Box(Round(10, C(0.15f, 0.15f, 0.20f, 0.9f)), 12);
        Toggle.hover.background = Round(10, C(0.19f, 0.19f, 0.26f));
        Toggle.onNormal.background = Round(10, new Color(ColorPink.r, ColorPink.g, ColorPink.b, 0.85f));
        Toggle.onHover.background = Round(10, ColorPink);
        Toggle.fontSize = 12;
        Toggle.alignment = TextAnchor.MiddleLeft;
        Toggle.padding = new RectOffset(12, 12, 5, 5);
        Toggle.margin = new RectOffset(0, 0, 4, 4);
        Toggle.normal.textColor = ColorMuted;
        Toggle.hover.textColor = ColorText;
        Toggle.onNormal.textColor = new Color(0.09f, 0.04f, 0.08f);
        Toggle.onHover.textColor = new Color(0.09f, 0.04f, 0.08f);

        TextField = Box(Round(8, C(0.10f, 0.10f, 0.14f)), 9);
        TextField.focused.background = Round(8, C(0.13f, 0.13f, 0.18f));
        TextField.fontSize = 12;
        TextField.alignment = TextAnchor.MiddleLeft;
        TextField.padding = new RectOffset(8, 8, 5, 5);
        TextField.margin = new RectOffset(0, 0, 3, 3);
        TextField.normal.textColor = ColorText;
        TextField.focused.textColor = ColorText;

        ErrorMessage = Label(11, ColorDanger, TextAnchor.MiddleLeft);
        ErrorMessage.wordWrap = true;

        DisablePanel = Label(13, ColorMuted, TextAnchor.MiddleCenter);
        DisablePanel.wordWrap = true;

        LineTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        LineTex.hideFlags = HideFlags.HideAndDontSave;
        LineTex.SetPixel(0, 0, new Color(1f, 1f, 1f, 0.07f));
        LineTex.Apply();
    }

    private GUIStyle Label(int size, Color color, TextAnchor anchor)
    {
        GUIStyle style = new GUIStyle();
        style.font = Font;
        style.fontSize = size;
        style.alignment = anchor;
        style.normal.textColor = color;
        style.richText = false;
        return style;
    }

    private GUIStyle Box(Texture2D background, int border)
    {
        GUIStyle style = new GUIStyle();
        style.font = Font;
        style.normal.background = background;
        style.border = new RectOffset(border, border, border, border);
        return style;
    }

    private static Texture2D Round(int radius, Color fill)
    {
        int size = radius * 2 + 8;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = 0f;
                float dy = 0f;
                if (x < radius)
                {
                    dx = radius - x;
                }
                else if (x >= size - radius)
                {
                    dx = x - (size - radius - 1);
                }
                if (y < radius)
                {
                    dy = radius - y;
                }
                else if (y >= size - radius)
                {
                    dy = y - (size - radius - 1);
                }
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01(radius - distance + 0.5f);
                Color color = fill;
                color.a *= alpha;
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
        return texture;
    }
}
