# Builds launcher\PriorityAgent.exe (self-contained, single-file, win-x64, Priority icon embedded)
# Optional: -Install copies it to -Dest (default D:\tools\PriorityAgent) then runs PriorityAgent.exe --install
# so the Desktop shortcut uses the same FR #668 path as a downloaded release.
# Optional: -Version X.Y.Z (default 1.0.0) stamps FileVersion X.Y.Z.0 and InformationalVersion X.Y.Z+<sha>.
param(
    [switch]$Install,
    [string]$Dest = 'D:\tools\PriorityAgent',
    [string]$Version = ''
)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Get-LauncherReleaseVersion.ps1')
if (-not $Version) { $Version = Get-LauncherReleaseVersion -Tag '' -Fallback '1.0.0' }
$fileVer = Get-LauncherFileVersion -Version $Version
$sha = ''
try { $sha = (git -C $PSScriptRoot rev-parse HEAD 2>$null) } catch { $sha = '' }
$info = Get-LauncherInformationalVersion -Version $Version -Sha $sha
Push-Location $PSScriptRoot
try {
    dotnet publish PriorityAgent.csproj -c Release -o out `
        "-p:Version=$Version" "-p:FileVersion=$fileVer" "-p:InformationalVersion=$info"
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }
    Write-Host "Built $PSScriptRoot\out\PriorityAgent.exe ($Version / $fileVer / $info)"
    if ($Install) {
        New-Item -ItemType Directory -Force -Path $Dest | Out-Null
        Copy-Item out\PriorityAgent.exe, priority.ico -Destination $Dest -Force
        $exe = Join-Path $Dest 'PriorityAgent.exe'
        & $exe --install
        if ($LASTEXITCODE -ne 0) { throw "PriorityAgent.exe --install failed ($LASTEXITCODE)" }
        Write-Host "Installed to $Dest (FR #668 desktop shortcut via --install)"
    }
} finally { Pop-Location }
