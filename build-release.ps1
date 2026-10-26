[CmdletBinding()]
param(
    [string]$Version,
    [switch]$Bump,
    [string]$Dotnet,
    [switch]$Deploy,
    [switch]$Commit,
    [switch]$Tag,
    [switch]$Push
)

$ErrorActionPreference = 'Stop'

$repo = $PSScriptRoot
$src = Join-Path $repo 'src'
$gameDir = Split-Path $repo -Parent

$assemblyInfoPath = Join-Path $src 'Properties\AssemblyInfo.cs'
$buildInfoPath = Join-Path $src 'SinmaiAssist\BuildInfo.cs'
$mainGuiPath = Join-Path $src 'SinmaiAssist.GUI\MainGUI.cs'
$versionInfoPath = Join-Path $src 'SinmaiAssist.GUI\ShowVersionInfo.cs'

function Write-Step([string]$Text) {
    Write-Host "==> $Text" -ForegroundColor Cyan
}

function Fail([string]$Text) {
    Write-Host "ERROR: $Text" -ForegroundColor Red
    exit 1
}

function Read-Text([string]$Path) {
    return [System.IO.File]::ReadAllText($Path, [System.Text.Encoding]::UTF8)
}

function Write-Text([string]$Path, [string]$Text) {
    [System.IO.File]::WriteAllText($Path, $Text, (New-Object System.Text.UTF8Encoding($false)))
}

$assemblyInfo = Read-Text $assemblyInfoPath
$match = [regex]::Match($assemblyInfo, 'AssemblyFileVersion\("([\d.]+)"\)')
if (-not $match.Success) {
    Fail "Cannot find AssemblyFileVersion in $assemblyInfoPath"
}
$currentVersion = $match.Groups[1].Value

if (-not $Version) {
    if ($Bump) {
        $parts = $currentVersion.Split('.')
        $parts[$parts.Length - 1] = [string]([int]$parts[$parts.Length - 1] + 1)
        $Version = $parts -join '.'
    } else {
        $Version = $currentVersion
    }
}

if ($Version -notmatch '^\d+\.\d+\.\d+(\.\d+)?$') {
    Fail "Invalid version '$Version'. Use a format like 1.0.1"
}

$assemblyVersion = $Version
if (($Version.Split('.')).Count -eq 3) {
    $assemblyVersion = "$Version.0"
}

Write-Step "Version: $currentVersion -> $Version"

$assemblyInfo = $assemblyInfo -replace 'AssemblyFileVersion\("[\d.]+"\)', "AssemblyFileVersion(`"$Version`")"
$assemblyInfo = $assemblyInfo -replace 'AssemblyVersion\("[\d.]+"\)', "AssemblyVersion(`"$assemblyVersion`")"
$assemblyInfo = $assemblyInfo -replace '("Sinmai-Assist",\s*")[\d.]+(")', "`${1}$Version`${2}"
Write-Text $assemblyInfoPath $assemblyInfo

$buildInfo = Read-Text $buildInfoPath
$buildInfo = $buildInfo -replace '(Version\s*=\s*")[\d.]+(")', "`${1}$Version`${2}"
$buildInfo = $buildInfo -replace '(BuildDate\s*=\s*")[^"]*(")', "`${1}$((Get-Date).ToString('yyyy-MM-ddTHH:mm:ss.fffzzz'))`${2}"
Write-Text $buildInfoPath $buildInfo

$mainGui = Read-Text $mainGuiPath
$mainGui = $mainGui -replace '(maimai DX 助手 ·\s*)[\d.]+', "`${1}$Version"
Write-Text $mainGuiPath $mainGui

$versionInfo = Read-Text $versionInfoPath
$versionInfo = $versionInfo -replace '("Sinmai-Assist )[\d.]+( \()', "`${1}$Version`${2}"
Write-Text $versionInfoPath $versionInfo

$dotnetExe = $null
if ($Dotnet) {
    if (-not (Test-Path $Dotnet)) {
        Fail "dotnet not found: $Dotnet"
    }
    $dotnetExe = (Resolve-Path $Dotnet).Path
} else {
    $command = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($command) {
        $sdks = & $command.Source --list-sdks 2>$null
        if ($LASTEXITCODE -eq 0 -and $sdks) {
            $dotnetExe = $command.Source
        }
    }
    if (-not $dotnetExe) {
        $fallback = Join-Path $gameDir 'Sinmai-Assist-build\dotnet\dotnet.exe'
        if (Test-Path $fallback) {
            $dotnetExe = $fallback
        }
    }
}

if (-not $dotnetExe) {
    Fail "dotnet SDK not found. Install .NET SDK 8 or pass -Dotnet <path to dotnet.exe>"
}

Write-Step "Building with $dotnetExe"
& $dotnetExe build (Join-Path $src 'Sinmai-Assist.csproj') -c Release -v m "-p:GameDir=$gameDir" "-p:MelonDir=$gameDir"
if ($LASTEXITCODE -ne 0) {
    Fail "Build failed"
}

$dll = Join-Path $src 'bin\Release\net472\Sinmai-Assist.dll'
if (-not (Test-Path $dll)) {
    Fail "Build output not found: $dll"
}

$dist = Join-Path $repo 'dist'
New-Item -ItemType Directory -Force -Path $dist | Out-Null

$stage = Join-Path $env:TEMP ("sinmai-assist-" + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $stage | Out-Null

Copy-Item $dll (Join-Path $stage 'Sinmai-Assist.dll')

$installText = @"
Sinmai-Assist 安装说明

版本：$Version

1. 关闭游戏
2. 备份游戏目录下的 Mods\Sinmai-Assist.dll（如果有）
3. 把本压缩包里的 Sinmai-Assist.dll 解压到 Mods 目录，覆盖原文件
4. 启动游戏

环境：MelonLoader 0.6.x

行为开关在「自动演奏」面板顶部：
- 打开「原版全自动（旧行为）」= 原版 mod 的全自动演奏
- 关闭 = 仅判定模式：自己打，命中后强制判定，星星要自己滑
"@

[System.IO.File]::WriteAllText((Join-Path $stage '安装说明.txt'), $installText, (New-Object System.Text.UTF8Encoding($true)))

$zip = Join-Path $dist "Sinmai-Assist-v$Version.zip"
Remove-Item $zip -Force -ErrorAction SilentlyContinue
Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zip -CompressionLevel Optimal
Remove-Item $stage -Recurse -Force

Write-Step "Package: $zip"

if ($Deploy) {
    $mods = Join-Path $gameDir 'Mods'
    $target = Join-Path $mods 'Sinmai-Assist.dll'
    if (Test-Path $target) {
        Copy-Item $target "$target.bak" -Force
    }
    Copy-Item $dll $target -Force
    Write-Step "Deployed to $target"
}

if ($Commit -or $Tag -or $Push) {
    Push-Location $repo
    try {
        if ($Commit) {
            Write-Step "git commit"
            & git add -A
            if ($LASTEXITCODE -ne 0) { throw "git add failed" }
            & git commit -m "release v$Version"
            if ($LASTEXITCODE -ne 0) { throw "git commit failed" }
        }
        if ($Tag) {
            Write-Step "git tag v$Version"
            & git tag "v$Version"
            if ($LASTEXITCODE -ne 0) { throw "git tag failed" }
        }
        if ($Push) {
            Write-Step "git push"
            & git push
            if ($LASTEXITCODE -ne 0) { throw "git push failed" }
            if ($Tag) {
                & git push --tags
                if ($LASTEXITCODE -ne 0) { throw "git push --tags failed" }
            }
        }
    } finally {
        Pop-Location
    }
}

Write-Host ""
Write-Host "Done: v$Version" -ForegroundColor Green
Write-Host "  DLL: $dll"
Write-Host "  ZIP: $zip"
