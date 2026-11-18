#requires -Version 5.1
# FR #699: catalog skill priority-winhtml-download is listed and hostile-pinned.
$ErrorActionPreference = 'Stop'
$repo = if ($env:FOMPREP_MRB_ROOT) { $env:FOMPREP_MRB_ROOT } else { Split-Path -Parent $PSScriptRoot }
$failed = 0
function Gate([string]$Id, [bool]$Pass, [string]$Detail) {
  if ($Pass) { Write-Host "PASS $Id $Detail" }
  else { Write-Host "FAIL $Id $Detail"; $script:failed++ }
}
$src = Join-Path $repo 'docs\skill-sources\programming\WINHTML_DOWNLOAD_PATHS.md'
$catSkill = Join-Path $repo 'v2\apps\mcp-catalog\catalog\priority-winhtml-download\SKILL.md'
$catMeta = Join-Path $repo 'v2\apps\mcp-catalog\catalog\priority-winhtml-download\meta.json'
$grokSkill = Join-Path $repo '.grok\skills\priority-winhtml-download\SKILL.md'
$manifest = Join-Path $repo 'docs\skill-sources\programming\MANIFEST.md'
$catalogGate = Join-Path $repo 'v2\tools\Test-PriorityCatalog.ps1'
Gate 'src' (Test-Path $src) $src
Gate 'catalog-skill' (Test-Path $catSkill) $catSkill
Gate 'catalog-meta' (Test-Path $catMeta) $catMeta
Gate 'grok-mirror' (Test-Path $grokSkill) $grokSkill
$sk = [IO.File]::ReadAllText($catSkill)
$md = [IO.File]::ReadAllText($src)
$blob = $sk + $md
Gate 'get-skill-id' ($sk -match 'priority-winhtml-download') 'catalog list / get_skill name'
Gate 'foundation' ($sk -match 'Foundation: harvest-priority-skills' -and $sk -match 'SimonBarnett/priority-mcp') 'honesty-box foundation line'
Gate 'system-mail' ($blob -match '\.\./\.\./system/mail') 'prefer ../../system/mail'
Gate 'download-or-mailmsg' ($blob -match 'DOWNLOAD' -and $blob -match 'MAILMSG') 'DOWNLOAD + MAILMSG'
Gate 'pdf-flag' ($blob -match "-pdf") 'WINHTML -pdf flag'
Gate 'excel-not-winhtml' ($blob -match 'WINACTIVF' -or $blob -match 'Excel is not WINHTML') 'Excel via WINACTIVF not WINHTML'
Gate 'saas-vs-onprem' ($blob -match '(?i)SaaS' -and $blob -match '(?i)on-prem') 'SaaS vs on-prem'
Gate 'no-password-eq' ($blob -notmatch '(?i)password\s*=') 'no password= secret literal (FR #3304)'
Gate 'manifest-row' ((Test-Path $manifest) -and ([IO.File]::ReadAllText($manifest) -match 'WINHTML_DOWNLOAD_PATHS\.md' -and [IO.File]::ReadAllText($manifest) -match 'priority-winhtml-download')) 'MANIFEST lists skill'
Gate 'catalog-gate-lists' ((Test-Path $catalogGate) -and ([IO.File]::ReadAllText($catalogGate) -match 'priority-winhtml-download')) 'Test-PriorityCatalog expected includes skill'
if ($failed -gt 0) { Write-Host "Test-Fr699-WinhtmlDownload FAIL ($failed)"; exit 1 }
Write-Host 'Test-Fr699-WinhtmlDownload PASS'
exit 0
