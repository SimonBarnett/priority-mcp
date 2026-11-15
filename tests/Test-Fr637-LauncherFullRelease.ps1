# FR #637: launcher-v* GitHub releases must be non-prerelease so /downloads can
# link PriorityAgent.exe without relying on prerelease inclusion.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$wf = Join-Path $root '.github\workflows\launcher-release.yml'
if (-not (Test-Path -LiteralPath $wf)) { throw "missing $wf" }
$text = Get-Content -LiteralPath $wf -Raw -Encoding utf8

if ($text -notmatch 'PriorityAgent\.exe') { throw 'workflow must publish PriorityAgent.exe' }
if ($text -notmatch 'PriorityAgent\.exe\.sha256') { throw 'workflow must publish PriorityAgent.exe.sha256' }

# Must create a full release (no bare --prerelease on create; --prerelease=false promote is OK).
if ($text -match '(?m)--prerelease(\s|$|`)') {
    throw 'launcher-release.yml still passes --prerelease on create; FR #637 requires a full release'
}
if ($text -notmatch '--prerelease=false --latest') {
    throw 'workflow should promote existing tags with --prerelease=false --latest'
}
if ($text -notmatch 'GitHub release upload failed') {
    throw 'workflow must fail closed when gh release upload fails before promote'
}

if ($text -notmatch 'if \(\$release\.isPrerelease\)') {
    throw 'workflow must fail if the published release is still a prerelease'
}
if ($text -notmatch 'FR #637 requires a full release') {
    throw 'workflow must cite FR #637 full-release gate'
}

Write-Host 'PASS FR #637 launcher-release publishes full release with PriorityAgent.exe'
exit 0
