# Sinmai-Assist（判定增强 + 中文界面版）

基于 [WYH2004-MC/Sinmai-Assist](https://github.com/WYH2004-MC/Sinmai-Assist) 1.0.0（commit `787a437`）的修改版，适用于 maimai DX Sinmai（MelonLoader）。

> 说明：本仓库源码是从原版 `Sinmai-Assist.dll` 反编译后修改得到的（原仓库没有与该 DLL 完全对应的源码快照）。如果你想向上游提交，建议 fork 上游仓库后只移植 `src/SinmaiAssist.Cheat/AutoPlay.cs` 和 `src/SinmaiAssist.GUI/` 下的改动。

## 改了什么

### 1. Autoplay 只改判定，不自动演奏

- 不再强制游戏进入 AutoPlay 状态，触摸输入恢复正常（原版会每帧把所有触摸区标记为已使用）。
- tap / hold / break 由玩家自己打；命中后判定强制为所选模式（Critical / Perfect / Great / Good / 随机…）。
- 星星（slide）必须自己滑：不滑或滑太晚会 Miss，滑完才给判定。
- touch 触摸圆点保持自动判定，不需要按。
- 没打中仍然正常 Miss。

### 2. UI

- 深色半透明面板 + 圆角卡片 + 粉色/青色强调色。
- 全部面板中文化，使用系统「微软雅黑」动态字体。
- 面板可拖动，内容支持滚轮滚动。

### 3. 行为开关

「自动演奏」面板顶部有 **「原版全自动（旧行为）」** 开关：

- 打开：恢复原版 Sinmai-Assist 行为（自动打所有 note、星星自动滑、触摸被屏蔽）。
- 关闭：仅判定模式（自己打、命中后只改判定、星星自己滑）。

开关会写入 `Sinmai-Assist/Config.yml` 的 `cheat.classicAutoPlay`，重启游戏后保持。

## 构建

需要 .NET SDK 8（或更高）。

本仓库不包含游戏 DLL。构建时用 `GameDir` / `MelonDir` 指向游戏目录：

```powershell
dotnet build .\src\Sinmai-Assist.csproj -c Release `
  -p:GameDir="D:\GameVideos\game\MAIMAI\sdgb" `
  -p:MelonDir="D:\GameVideos\game\MAIMAI\sdgb"
```

默认值 `..\..` 表示仓库放在 `sdgb\Sinmai-Assist-source`（与 `Sinmai_Data`、`MelonLoader` 同级）。

需要的引用：

- `Sinmai_Data\Managed\*.dll`（Assembly-CSharp、UnityEngine 各模块、AMDaemon.NET、ChimeLib.NET 等）
- `MelonLoader\net35\MelonLoader.dll`、`0Harmony.dll`

输出：`src\bin\Release\net472\Sinmai-Assist.dll`，复制到游戏的 `Mods` 目录即可。

## 目录

- `src/SinmaiAssist.Cheat/AutoPlay.cs` — 判定逻辑
- `src/SinmaiAssist.GUI/` — 界面（Style / MainGUI / 各面板）
- `src/SinmaiAssist.Config/`、`src/SinmaiAssist.Common/`、`src/SinmaiAssist.Fix/` 等 — 原版功能
- `src/YamlDotNet*` — 内嵌的 YamlDotNet（MIT）

## 已知限制

- touch hold（长按触摸圈）目前仍需自己按；只有 touch tap 圆点是自动判定。
- 如果进歌后模式被游戏重置，请在面板里打开「锁定模式」。

## 许可与致谢

- 原项目：[WYH2004-MC/Sinmai-Assist](https://github.com/WYH2004-MC/Sinmai-Assist)。上游仓库未见 LICENSE 文件，公开再分发前请先征得原作者同意。
- [YamlDotNet](https://github.com/aaubry/YamlDotNet)：MIT License。
- 本仓库不包含任何游戏资源或游戏 DLL。
