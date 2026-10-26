using System;
using MAI2.Util;
using Manager;
using MelonLoader;
using SinmaiAssist.Utils;
using UnityEngine;

namespace SinmaiAssist.GUI;

public class UserDataPanel
{
    private enum CollectionType
    {
        Icon = 1,
        Plate,
        Title,
        Partner,
        Frame
    }

    private static UserData _player1 = null;

    private static UserData _player2 = null;

    private static bool _isNewItem = false;

    private static string[] _userInputId = new string[10] { "", "", "", "", "", "", "", "", "", "" };

    public static void OnGUI()
    {
        GUILayout.Label("用户信息", MainGUI.Style.Title);
        try
        {
            _player1 = Singleton<UserDataManager>.Instance.GetUserData(0L);
            _player2 = Singleton<UserDataManager>.Instance.GetUserData(1L);
        }
        catch (Exception)
        {
        }
        GUILayout.Label($"1P：{_player1.Detail.UserName}（{_player1.Detail.UserID}）", MainGUI.Style.Text);
        GUILayout.Label($"2P：{_player2.Detail.UserName}（{_player2.Detail.UserID}）", MainGUI.Style.Text);

        GUILayout.Label("添加收藏品", MainGUI.Style.Title);
        foreach (CollectionType value in Enum.GetValues(typeof(CollectionType)))
        {
            GUILayout.BeginHorizontal();
            int num = (int)value;
            GUILayout.Label(TypeName(value), new GUIStyle(MainGUI.Style.Text)
            {
                fixedWidth = 56f
            });
            _userInputId[num] = GUILayout.TextField(_userInputId[num], MainGUI.Style.TextField, GUILayout.Height(26f));
            if (GUILayout.Button("添加", new GUIStyle(MainGUI.Style.Button)
            {
                fixedWidth = 56f
            }, GUILayout.Height(26f)))
            {
                AddCollections(0L, value, _userInputId[num]);
                AddCollections(1L, value, _userInputId[num]);
            }
            GUILayout.EndHorizontal();
        }
        _isNewItem = GUILayout.Toggle(_isNewItem, "标记为新物品", MainGUI.Style.Toggle, GUILayout.Height(26f));

        GUILayout.Label("解锁乐曲", MainGUI.Style.Title);
        GUILayout.BeginHorizontal();
        GUILayout.Label("乐曲 ID", new GUIStyle(MainGUI.Style.Text)
        {
            fixedWidth = 56f
        });
        _userInputId[0] = GUILayout.TextField(_userInputId[0], MainGUI.Style.TextField, GUILayout.Height(26f));
        if (GUILayout.Button("添加", new GUIStyle(MainGUI.Style.Button)
        {
            fixedWidth = 56f
        }, GUILayout.Height(26f)))
        {
            UnlockMusic(0L, _userInputId[0]);
            UnlockMusic(1L, _userInputId[0]);
        }
        GUILayout.EndHorizontal();

        GUILayout.Label("MaiMile", MainGUI.Style.Title);
        GUILayout.BeginHorizontal();
        GUILayout.Label("数量", new GUIStyle(MainGUI.Style.Text)
        {
            fixedWidth = 56f
        });
        _userInputId[6] = GUILayout.TextField(_userInputId[6], MainGUI.Style.TextField, GUILayout.Height(26f));
        if (GUILayout.Button("添加", new GUIStyle(MainGUI.Style.Button)
        {
            fixedWidth = 56f
        }, GUILayout.Height(26f)))
        {
            AddMaiMile(0L, _userInputId[6]);
            AddMaiMile(1L, _userInputId[6]);
        }
        GUILayout.EndHorizontal();

        GUILayout.Label("Kaleidx Scope", MainGUI.Style.Title);
        GUILayout.BeginHorizontal();
        GUILayout.Label("门", new GUIStyle(MainGUI.Style.Text)
        {
            fixedWidth = 56f
        });
        _userInputId[7] = GUILayout.TextField(_userInputId[7], MainGUI.Style.TextField, GUILayout.Height(26f));
        if (GUILayout.Button("添加", new GUIStyle(MainGUI.Style.Button)
        {
            fixedWidth = 56f
        }, GUILayout.Height(26f)))
        {
            AddKaleidxScopeGate(0L, _userInputId[7]);
            AddKaleidxScopeGate(1L, _userInputId[7]);
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("钥匙", new GUIStyle(MainGUI.Style.Text)
        {
            fixedWidth = 56f
        });
        _userInputId[8] = GUILayout.TextField(_userInputId[8], MainGUI.Style.TextField, GUILayout.Height(26f));
        if (GUILayout.Button("添加", new GUIStyle(MainGUI.Style.Button)
        {
            fixedWidth = 56f
        }, GUILayout.Height(26f)))
        {
            AddKaleidxScopeKey(0L, _userInputId[8]);
            AddKaleidxScopeKey(1L, _userInputId[8]);
        }
        GUILayout.EndHorizontal();

        GUILayout.Label("用户数据备份", MainGUI.Style.Title);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("导出 1P", MainGUI.Style.Button, GUILayout.Height(30f)))
        {
            User.ExportBackupData(0L);
        }
        if (GUILayout.Button("导出 2P", MainGUI.Style.Button, GUILayout.Height(30f)))
        {
            User.ExportBackupData(1L);
        }
        GUILayout.EndHorizontal();
    }

    private static string TypeName(CollectionType type)
    {
        return type switch
        {
            CollectionType.Icon => "图标",
            CollectionType.Plate => "铭牌",
            CollectionType.Title => "称号",
            CollectionType.Partner => "伙伴",
            CollectionType.Frame => "边框",
            _ => type.ToString()
        };
    }

    private static void AddCollections(long index, CollectionType type, string input)
    {
        UserData userData = Singleton<UserDataManager>.Instance.GetUserData(index);
        if (userData.IsGuest())
        {
            GameMessageManager.SendMessage((int)index, "游客账号\n无法添加收藏品");
            return;
        }
        try
        {
            if (int.TryParse(input, out var result))
            {
                if (userData.AddCollections((UserData.Collection)type, result, _isNewItem))
                {
                    GameMessageManager.SendMessage((int)index, $"添加收藏品\n{TypeName(type)} {result}" + (_isNewItem ? "（新物品）" : ""));
                }
                else
                {
                    GameMessageManager.SendMessage((int)index, $"添加失败或已拥有\n{TypeName(type)} {result}");
                }
            }
            else
            {
                GameMessageManager.SendMessage((int)index, "无效 ID\n" + input);
            }
        }
        catch (Exception obj)
        {
            GameMessageManager.SendMessage((int)index, "未知错误");
            MelonLogger.Error(obj);
        }
    }

    private static void UnlockMusic(long index, string input)
    {
        UserData userData = Singleton<UserDataManager>.Instance.GetUserData(index);
        if (userData.IsGuest())
        {
            GameMessageManager.SendMessage((int)index, "游客账号\n无法解锁乐曲");
            return;
        }
        try
        {
            if (int.TryParse(input, out var result))
            {
                if (!userData.IsUnlockMusic(UserData.MusicUnlock.Base, result))
                {
                    if (userData.AddUnlockMusic(UserData.MusicUnlock.Base, result))
                    {
                        GameMessageManager.SendMessage((int)index, $"解锁乐曲\n{result}");
                    }
                    else
                    {
                        GameMessageManager.SendMessage((int)index, $"解锁失败或已解锁\n{result}");
                    }
                }
                else if (!userData.IsUnlockMusic(UserData.MusicUnlock.Master, result))
                {
                    userData.AddUnlockMusic(UserData.MusicUnlock.Master, result);
                    userData.AddUnlockMusic(UserData.MusicUnlock.ReMaster, result);
                    GameMessageManager.SendMessage((int)index, $"解锁 Master\n{result}");
                }
                else
                {
                    GameMessageManager.SendMessage((int)index, $"解锁 Master 失败或已解锁\n{result}");
                }
            }
            else
            {
                GameMessageManager.SendMessage((int)index, "无效 ID\n" + input);
            }
        }
        catch (Exception obj)
        {
            GameMessageManager.SendMessage((int)index, "未知错误");
            MelonLogger.Error(obj);
        }
    }

    private static void AddMaiMile(long index, string input)
    {
        UserData userData = Singleton<UserDataManager>.Instance.GetUserData(index);
        if (SinmaiAssist.GameVersion < 25000)
        {
            GameMessageManager.SendMessage((int)index, "当前版本不支持 MaiMile");
            return;
        }
        if (userData.IsGuest())
        {
            GameMessageManager.SendMessage((int)index, "游客账号\n无法添加 MaiMile");
            return;
        }
        try
        {
            if (int.TryParse(input, out var result))
            {
                int point = userData.Detail.Point;
                if (point + result >= 99999)
                {
                    result = 99999 - point;
                }
                int num = point + result;
                userData.AddPresentMile(result);
                GameMessageManager.SendMessage((int)index, $"添加 {result} MaiMile\n（{point} -> {num}）");
            }
            else
            {
                GameMessageManager.SendMessage((int)index, "无效的 MaiMile 数量\n" + input);
            }
        }
        catch (Exception obj)
        {
            GameMessageManager.SendMessage((int)index, "未知错误");
            MelonLogger.Error(obj);
        }
    }

    private static void AddKaleidxScopeGate(long index, string input)
    {
        UserData userData = Singleton<UserDataManager>.Instance.GetUserData(index);
        if (userData.IsGuest())
        {
            GameMessageManager.SendMessage((int)index, "游客账号\n无法添加门");
            return;
        }
        try
        {
            if (int.TryParse(input, out var result))
            {
                userData.AddKaleidxScopeGate(result);
                GameMessageManager.SendMessage((int)index, $"添加 Kaleidx Scope 门\n{result}");
            }
            else
            {
                GameMessageManager.SendMessage((int)index, "无效 ID\n" + input);
            }
        }
        catch (Exception obj)
        {
            GameMessageManager.SendMessage((int)index, "未知错误");
            MelonLogger.Error(obj);
        }
    }

    private static void AddKaleidxScopeKey(long index, string input)
    {
        UserData userData = Singleton<UserDataManager>.Instance.GetUserData(index);
        if (userData.IsGuest())
        {
            GameMessageManager.SendMessage((int)index, "游客账号\n无法添加钥匙");
            return;
        }
        try
        {
            if (int.TryParse(input, out var result))
            {
                userData.AddKaleidxScopeKey(result);
                GameMessageManager.SendMessage((int)index, $"添加 Kaleidx Scope 钥匙\n{result}");
            }
            else
            {
                GameMessageManager.SendMessage((int)index, "无效 ID\n" + input);
            }
        }
        catch (Exception obj)
        {
            GameMessageManager.SendMessage((int)index, "未知错误");
            MelonLogger.Error(obj);
        }
    }
}
