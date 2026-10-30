using System.Collections.Generic;
using UnityEngine;

namespace SinmaiAssist.GUI;

public static class I18n
{
    public static bool EnglishMode;

    private static readonly Dictionary<string, string> English = new Dictionary<string, string>
    {
        { "不包含 Break：绝赞音符固定为 Critical Perfect，非 Critical 判定只会在普通音符中抽取。", "Exclude Break: Break notes are always Critical Perfect; non-Critical judgments are selected only from normal notes." },
        { "严格数量模式：整首歌会恰好生成指定数量的非 Critical 判定，其余音符全部为 Critical Perfect。", "Exact-count mode: the chart generates exactly the specified number of non-Critical judgments; all remaining notes are Critical Perfect." },
        { "其余音符：Critical Perfect。小P 只会是 Perfect，不会出现 Critical Perfect。", "All other notes: Critical Perfect. Small P is always Perfect, never Critical Perfect." },
        { "只修改判定结果，不会替你演奏。tap / hold / 星星都需要你自己打；touch 触摸圆点保持自动判定。", "Only the judgment result is changed; it does not play for you. Tap / hold / slides must be played manually; touch notes remain automatic." },
        { "客户端版本：{SinmaiAssist.GameID} {SinmaiAssist.GameVersion}", "Client version: {SinmaiAssist.GameID} {SinmaiAssist.GameVersion}" },
        { "Slide（滑动）音符不会参与小P抽取；其它非 Critical 判定仍可落在 Slide 上。", "Slide notes are excluded from small-P selection; other non-Critical judgments can still land on slides." },
        { "Slide（滑动）音符不会参与小P抽取；Great/Good/Miss 是否可落在 Slide 上由各自开关控制。", "Slide notes are excluded from small-P selection; whether Great/Good/Miss can land on slides is controlled by their own toggles." },
        { "BreakSlide（绝赞 Slide）需要同时开启 Break 和对应 Slide 开关。", "BreakSlide requires both the Break toggle and the matching Slide toggle." },
        { "Great 包含 Slide（滑动）", "Great includes Slide" },
        { "Good 包含 Slide（滑动）", "Good includes Slide" },
        { "Miss 包含 Slide（滑动）", "Miss includes Slide" },
        { "数量超过谱面可抽取音符时会自动截断；全为 0 时整首歌为 Critical Perfect。", "The count is clamped when it exceeds the available notes; all zeros means the entire chart is Critical Perfect." },
        { "请先在 Sinmai-Assist/Config.yml 中开启对应功能，然后重启游戏。", "Enable the feature in Sinmai-Assist/Config.yml, then restart the game." },
        { "提示：0 表示该判定不出现；未列出的音符全部是 Critical Perfect。", "Tip: 0 means the judgment does not appear; all other notes are Critical Perfect." },
        { "请选择一个随机模式，然后设置本曲需要的小P、Great、Good、Miss 数量。", "Select a random mode, then set the small-P, Great, Good, and Miss counts for this song." },
        { "注意：仅判定模式下，没打中的音符仍然是 Miss；方案只决定你打中后的判定。", "Note: in Judge-only mode, missed notes are still Miss; the plan only determines the judgment after you hit." },
        { "包含 Break：本模式的非 Critical 判定都可能落在绝赞音符上。", "Include Break: non-Critical judgments in this mode may land on Break notes." },
        { "当前：原版全自动 —— 自动打所有 note、自动滑星星，触摸会被屏蔽。", "Current: Classic Auto Play — all notes and slides are automatic; touch is blocked." },
        { "其余音符：Critical Perfect。Miss 会按未击中处理。", "All other notes: Critical Perfect. Miss is treated as a missed note." },
        { "当前：仅判定模式 —— 自己打，命中后强制判定；星星要自己滑。", "Current: Judge-only — play manually; judgment is forced after hit; slides must be moved manually." },
        { "AMDaemon 启动时间：{Auth.AuthTime}", "AMDaemon start time: {Auth.AuthTime}" },
        { "当前行为：仅判定（需要自己打，命中后才套用判定）", "Current behavior: Judge-only (play manually; judgment applied after hit)" },
        { "其余音符：Critical Perfect。", "All other notes: Critical Perfect." },
        { "锁定当前模式（不跟随游戏自动演奏状态切换）", "Lock current mode (do not follow in-game auto-play state)" },
        { "错误：请输入 0 到 101 之间的数值。", "Error: enter a value between 0 and 101." },
        { "maimai DX 助手 · 1.0.0", "maimai DX Assistant · 1.0.0" },
        { "当前行为：原版全自动（所有音符自动判定）", "Current behavior: Classic Auto Play (all notes auto-judged)" },
        { "锁定模式（不跟随游戏自动演奏状态切换）", "Lock mode (do not follow in-game auto-play state)" },
        { "将当前乐曲直接结算为指定成绩。", "Finish the current song with the specified result." },
        { "当前分数：{result} %", "Current score: {result} %" },
        { "AMDaemon 启动时间：", "AMDaemon start time: " },
        { "包含 Break（绝赞音符）", "Include Break notes" },
        { "自定义分数（0 - 101）", "Custom score (0 - 101)" },
        { "随机 All Perfect", "Random All Perfect" },
        { "保存 P1 设置到默认选项", "Save P1 settings as default options" },
        { "保存 P2 设置到默认选项", "Save P2 settings as default options" },
        { "强制附加 1 个 Miss", "Force 1 Miss" },
        { "最大帧率（-1 为不限制）", "Max FPS (-1 = unlimited)" },
        { "记录 Unity 调试日志", "Log Unity debug output" },
        { "记录时间：{0}（{1}）", "Recorded time: {0} ({1})" },
        { "自定义 Autoplay", "Custom Autoplay" },
        { "错误：请输入有效的整数。", "Error: enter a valid integer." },
        { "切换 Utage 模式", "Toggle Utage mode" },
        { "小P（Perfect）", "Small P (Perfect)" },
        { "原版全自动（旧行为）", "Classic Auto Play (legacy)" },
        { "切换全屏 / 窗口", "Toggle fullscreen / window" },
        { "Great 数量", "Great count" },
        { "切换显示 FPS", "Toggle FPS display" },
        { "用户 ID 登录", "Login by user ID" },
        { "Good 数量", "Good count" },
        { "Miss 数量", "Miss count" },
        { "关闭自定义随机", "Turn off custom random" },
        { "本模式数量归零", "Reset this mode to zero" },
        { "配置尚未加载。", "Config not loaded." },
        { " 登出失败。", "Logout failed." },
        { " 登出成功。", "Logout succeeded." },
        { "切换显示信息", "Toggle info display" },
        { "发送测试消息", "Send test message" },
        { "客户端版本：", "Client version: " },
        { "当前服务器：", "Server: " },
        { "显示触摸区域", "Show touch areas" },
        { "标记为新物品", "Mark as new item" },
        { "用户 ID：", "User ID: " },
        { "用户数据备份", "User Data Backup" },
        { "计时器状态：", "Timer status: " },
        { "随机 FC+", "Random FC+" },
        { "乐曲 ID", "Music ID" },
        { "功能未启用", "Feature disabled" },
        { "导出 1P", "Export 1P" },
        { "导出 2P", "Export 2P" },
        { "当前分数：", "Current score: " },
        { "当前模式：", "Current mode: " },
        { "数据版本：", "Data version: " },
        { "添加收藏品", "Add Collection" },
        { "用户 ID", "User ID" },
        { "自定义画面", "Custom display" },
        { "自定义设置", "Custom settings" },
        { "记录时间：", "Recorded time: " },
        { "跳过模式：", "Skip mode: " },
        { "随机 FC", "Random FC" },
        { "判定模式", "Judgment Mode" },
        { "卡号登录", "Login by card" },
        { "安全模式", "Safe Mode" },
        { "快速跳过", "Fast Skip" },
        { "模式设置", "Mode settings" },
        { "测试工具", "Debug Tools" },
        { "用户信息", "User Info" },
        { "用户数据", "User Data" },
        { "用户登出", "Logout user" },
        { "界面开关", "UI toggles" },
        { "立即跳过", "Skip now" },
        { "自动演奏", "Auto Play" },
        { "虚拟登录", "Login" },
        { "行为模式", "Behavior" },
        { "解锁乐曲", "Unlock Music" },
        { "谱面控制", "Chart" },
        { "随机判定", "Random Judge" },
        { "1P：", "1P: " },
        { "2P：", "2P: " },
        { "已暂停", "Paused" },
        { "当前：", "Current: " },
        { "播放中", "Playing" },
        { "未播放", "Stopped" },
        { "自定义", "Custom" },
        { "错误：", "Error: " },
        { "伙伴", "Partner" },
        { "关闭", "Off" },
        { "卡号", "Card number" },
        { "图标", "Icon" },
        { "宽度", "Width" },
        { "应用", "Apply" },
        { "播放", "Play" },
        { "数量", "Amount" },
        { "暂停", "Pause" },
        { "添加", "Add" },
        { "画面", "Graphic" },
        { "称号", "Title" },
        { "计时", "Timer" },
        { "设置", "Set" },
        { "调试", "Debug" },
        { "边框", "Frame" },
        { "返回", "Back" },
        { "重置", "Reset" },
        { "钥匙", "Key" },
        { "铭牌", "Plate" },
        { "高度", "Height" },
        { "默认", "Default" },
        { "门", "Gate" },
        { "游客账号", "Guest account" },
        { "无法添加收藏品", "Cannot add collection item" },
        { "添加失败或已拥有", "Add failed or already owned" },
        { "无效 ID", "Invalid ID" },
        { "未知错误", "Unknown error" },
        { "无法解锁乐曲", "Cannot unlock music" },
        { "解锁失败或已解锁", "Unlock failed or already unlocked" },
        { "解锁 Master 失败或已解锁", "Unlock Master failed or already unlocked" },
        { "解锁 Master", "Unlock Master" },
        { "当前版本不支持 MaiMile", "This version does not support MaiMile" },
        { "无法添加 MaiMile", "Cannot add MaiMile" },
        { "无效的 MaiMile 数量", "Invalid MaiMile amount" },
        { "无法添加门", "Cannot add gate" },
        { "添加 Kaleidx Scope 门", "Add Kaleidx Scope gate" },
        { "无法添加钥匙", "Cannot add key" },
        { "添加 Kaleidx Scope 钥匙", "Add Kaleidx Scope key" },
        { "添加 ", "Add " },
        { "（新物品）", " (new item)" },
        { "：", ": " },
        { "（", " (" },
        { "）", ")" },
    };

    private static readonly string[] Keys = BuildKeys();

    private static string[] BuildKeys()
    {
        List<string> keys = new List<string>(English.Keys);
        keys.Sort((a, b) => b.Length.CompareTo(a.Length));
        return keys.ToArray();
    }

    public static string T(string text)
    {
        if (!EnglishMode || string.IsNullOrEmpty(text))
        {
            return text;
        }
        if (English.TryGetValue(text, out string exact))
        {
            return exact;
        }
        string result = text;
        foreach (string key in Keys)
        {
            if (result.Contains(key))
            {
                result = result.Replace(key, English[key]);
            }
        }
        return result;
    }

    public static string T(string chinese, string english)
    {
        return EnglishMode ? english : chinese;
    }

    public static void Label(string text, params GUILayoutOption[] options)
    {
        GUILayout.Label(T(text), options);
    }

    public static void Label(string text, GUIStyle style, params GUILayoutOption[] options)
    {
        GUILayout.Label(T(text), style, options);
    }

    public static bool Button(string text, params GUILayoutOption[] options)
    {
        return GUILayout.Button(T(text), options);
    }

    public static bool Button(string text, GUIStyle style, params GUILayoutOption[] options)
    {
        return GUILayout.Button(T(text), style, options);
    }

    public static bool Toggle(bool value, string text, params GUILayoutOption[] options)
    {
        return GUILayout.Toggle(value, T(text), options);
    }

    public static bool Toggle(bool value, string text, GUIStyle style, params GUILayoutOption[] options)
    {
        return GUILayout.Toggle(value, T(text), style, options);
    }
}
