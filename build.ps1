# Builds AtlyssTrainer.dll with the .NET Framework compiler (no SDK required).
# Usage:
#   .\build.ps1            -> build into .\dist
#   .\build.ps1 -Install   -> build and copy into BepInEx\plugins

param(
    [switch]$Install
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$game = "D:\SteamLibrary\steamapps\common\ATLYSS"
$managed = Join-Path $game "ATLYSS_Data\Managed"
$core = Join-Path $game "BepInEx\core"
$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

if (-not (Test-Path $csc)) { throw "csc.exe not found: $csc" }

$refs = @(
    (Join-Path $core "BepInEx.dll"),
    (Join-Path $core "0Harmony.dll"),
    (Join-Path $managed "netstandard.dll"),
    (Join-Path $managed "Assembly-CSharp.dll"),
    (Join-Path $managed "UnityEngine.dll"),
    (Join-Path $managed "UnityEngine.CoreModule.dll"),
    (Join-Path $managed "UnityEngine.UI.dll"),
    (Join-Path $managed "UnityEngine.UIModule.dll"),
    (Join-Path $managed "UnityEngine.InputLegacyModule.dll"),
    (Join-Path $managed "UnityEngine.TextRenderingModule.dll"),
    (Join-Path $managed "UnityEngine.AnimationModule.dll"),
    (Join-Path $managed "UnityEngine.PhysicsModule.dll"),
    (Join-Path $managed "UnityEngine.ImageConversionModule.dll"),
    (Join-Path $managed "Unity.TextMeshPro.dll"),
    (Join-Path $managed "Mirror.dll"),
    (Join-Path $managed "Newtonsoft.Json.dll")
)

foreach ($r in $refs) {
    if (-not (Test-Path $r)) { throw "Missing reference: $r" }
}

$sources = Get-ChildItem (Join-Path $root "src") -Recurse -Filter *.cs | ForEach-Object { $_.FullName }
if (-not $sources) { throw "No source files found in src" }

$dist = Join-Path $root "dist"
if (-not (Test-Path $dist)) { New-Item -ItemType Directory -Path $dist | Out-Null }

$out = Join-Path $dist "AtlyssTrainer.dll"
if (Test-Path $out) { Remove-Item $out -Force }

$refArgs = $refs | ForEach-Object { "/r:`"$_`"" }
$srcArgs = $sources | ForEach-Object { "`"$_`"" }

Write-Host "Compiling $($sources.Count) files..." -ForegroundColor Cyan
& $csc /nologo /target:library /langversion:5 /optimize+ /warn:2 /out:"$out" $refArgs $srcArgs

if ($LASTEXITCODE -ne 0) { throw "Compilation failed ($LASTEXITCODE)" }

Write-Host "Built: $out" -ForegroundColor Green

if ($Install) {
    $plugins = Join-Path $game "BepInEx\plugins"
    if (-not (Test-Path $plugins)) { New-Item -ItemType Directory -Path $plugins | Out-Null }
    Copy-Item $out $plugins -Force
    Write-Host "Installed to: $plugins" -ForegroundColor Green
}
