using AMDaemon.Allnet;
using Mai2.Mai2Cue;
using Mai2.Voice_000001;
using Manager;
using Net.Packet;
using Net.Packet.Helper;
using SinmaiAssist.Common;
using SinmaiAssist.Utils;
using UnityEngine;

namespace SinmaiAssist.GUI;

public class DummyLoginPanel
{
    public static string DummyLoginCode = "";

    public static string DummyUserId = "";

    public static bool CodeLoginFlag = false;

    public static bool UserIdLoginFlag = false;

    public static void OnGUI()
    {
        I18n.Label("卡号", MainGUI.Style.Title);
        DummyLoginCode = GUILayout.TextArea(DummyLoginCode, MainGUI.Style.TextField, GUILayout.Height(90f));
        if (I18n.Button("卡号登录", MainGUI.Style.Button, GUILayout.Height(32f)))
        {
            CodeLoginFlag = true;
            if (SinmaiAssist.GameID != "SDGB")
            {
                DummyAimeLogin.ReadCard();
            }
        }
        I18n.Label("用户 ID", MainGUI.Style.Title);
        DummyUserId = GUILayout.TextField(DummyUserId, MainGUI.Style.TextField, GUILayout.Height(28f));
        if (I18n.Button("用户 ID 登录", MainGUI.Style.Button, GUILayout.Height(32f)))
        {
            UserIdLoginFlag = true;
            if (SinmaiAssist.GameID != "SDGB")
            {
                DummyAimeLogin.ReadCard("12312312312312312312", DummyLoginCode);
            }
        }
        I18n.Label($"AMDaemon 启动时间：{Auth.AuthTime}", MainGUI.Style.Muted);
        if (SinmaiAssist.GameID == "SDGB" && I18n.Button("用户登出", MainGUI.Style.Button, GUILayout.Height(32f)))
        {
            PacketHelper.StartPacket(new UserLogout(ulong.Parse(DummyUserId), Auth.AuthTime, "", LogoutComplete, LogoutFailed));
        }
    }

    private static void LogoutComplete()
    {
        SoundManager.PlayVoice(Mai2.Voice_000001.Cue.VO_000012, 1);
        GameMessageManager.SendMessage(0, "Id: " + DummyUserId + " 登出成功。");
        GameMessageManager.SendMessage(1, "Id: " + DummyUserId + " 登出成功。");
    }

    private static void LogoutFailed(PacketStatus status)
    {
        SoundManager.PlaySE(Mai2.Mai2Cue.Cue.SE_ENTRY_AIME_ERROR, 1);
        GameMessageManager.SendMessage(0, "Id: " + DummyUserId + " 登出失败。");
        GameMessageManager.SendMessage(1, "Id: " + DummyUserId + " 登出失败。");
    }
}
