#requires -Version 5.1
# FR #688: Prepare Upgrade .sh is UTF-16 LE with BOM — document in shell skills.
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$failed = 0
function Gate([string]$Id, [bool]$Pass, [string]$Detail) {
  if ($Pass) { Write-Host "PASS $Id $Detail" }
  else { Write-Host "FAIL $Id $Detail"; $script:failed++ }
}

$skills = @(
  'priority-shell-compile',
  'priority-version-revision-discipline',
  'priority-shell-install'
)

foreach ($s in $skills) {
  $path = Join-Path $repo ".grok\skills\$s\SKILL.md"
  $text = [IO.File]::ReadAllText($path)
  Gate "$s-utf16" ($text -match 'UTF-16\s*LE') "$s mentions UTF-16 LE"
  Gate "$s-bom" ($text -match '\bBOM\b') "$s mentions BOM"
  Gate "$s-sha" ($text -match 'SHA-256|sha256') "$s mentions SHA-256 verify"
  Gate "$s-txt" ($text -match '\.sh\.txt|UTF-8.*review|review dump') "$s separates UTF-8 review dump from install binary"
  $cat = Join-Path $repo "v2\apps\mcp-catalog\catalog\$s\SKILL.md"
  if (Test-Path $cat) {
    $ct = [IO.File]::ReadAllText($cat)
    Gate "$s-catalog-sync" ($ct -eq $text) "$s catalog copy matches .grok/skills"
  }
}

if ($failed -gt 0) { Write-Host "Test-Fr688-ShellUtf16Bom FAIL ($failed)"; exit 1 }
Write-Host 'Test-Fr688-ShellUtf16Bom PASS'
exit 0