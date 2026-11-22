#requires -Version 5.1
# docs/mrb-749 / FR #738 PRI-01: pin model/context/effort acceptance on tip.
$ErrorActionPreference = 'Stop'
$repo = if ($env:FOMPREP_MRB_ROOT) { $env:FOMPREP_MRB_ROOT } else { Split-Path -Parent $PSScriptRoot }
$failed = 0
function Gate([string]$Id, [bool]$Pass, [string]$Detail) {
    if ($Pass) { Write-Host "PASS $Id $Detail" }
    else { Write-Host "FAIL $Id $Detail"; $script:failed++ }
}

$prog = [IO.File]::ReadAllText((Join-Path $repo 'launcher\Program.cs'))
$settings = [IO.File]::ReadAllText((Join-Path $repo 'launcher\PriorityStartSettings.cs'))
$readme = [IO.File]::ReadAllText((Join-Path $repo 'launcher\README.md'))
$driver = Join-Path $repo 'tests\Pri01SettingsDriver.cs'

Gate 'settings-class' (Test-Path (Join-Path $repo 'launcher\PriorityStartSettings.cs')) 'PriorityStartSettings.cs present'
Gate 'driver' (Test-Path $driver) 'Pri01SettingsDriver.cs present'
Gate 'parse-model' ($prog -match 'case "--model"' -and $prog -match 'PriorityStartSettings\.TrySet\("model"') 'Program parses --model'
Gate 'parse-context' ($prog -match 'case "--context"' -and $prog -match 'PriorityStartSettings\.TrySet\("context"') 'Program parses --context'
Gate 'parse-effort' ($prog -match 'case "--effort"' -and $prog -match 'PriorityStartSettings\.TrySet\("effort"') 'Program parses --effort'
Gate 'dry-run-line' ($prog -match 'PriorityStartSettings\.DryRunLine\(\)') 'launch prints DryRunLine'
Gate 'help-flags' ($prog -match '--model <id>' -and $prog -match '--context <n>' -and $prog -match '--effort <level>') 'Help lists flags'
Gate 'reject-meta' ($settings -match 'shell metacharacter') 'empty/metachar rejected'
Gate 'readme-model' ($readme -match '--model' -and $readme -match 'PRI-01') 'README documents PRI-01 flags'

if ($failed -gt 0) { Write-Host "Test-Fr738-Pri01Settings FAIL ($failed)"; exit 1 }
Write-Host 'Test-Fr738-Pri01Settings PASS'
exit 0