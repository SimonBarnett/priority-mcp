#requires -Version 5.1
# Customer repos never hold hours: setup templates, snippet and skill must not seed hours.json,
# hours files/folders or repo hours instructions; the only hours instruction is the Bob webhook pointer.
$ErrorActionPreference = 'Stop'
$repo = if ($env:FOMPREP_MRB_ROOT) { $env:FOMPREP_MRB_ROOT } else { Split-Path -Parent $PSScriptRoot }
$failed = 0
function Gate([string]$Id, [bool]$Pass, [string]$Detail) {
    if ($Pass) { Write-Host "PASS $Id $Detail" }
    else { Write-Host "FAIL $Id $Detail"; $script:failed++ }
}
function Read([string]$rel) { [IO.File]::ReadAllText((Join-Path $repo $rel)) }

$setupDir = Join-Path $repo 'launcher\customer-setup'
$files = @(Get-ChildItem -LiteralPath $setupDir -Recurse -File)
$hoursNamed = @($files | Where-Object { $_.Name -match '(?i)hours' -or $_.FullName -match '(?i)[\\/]hours[\\/]' })
Gate 'no-hours-template-files' ($hoursNamed.Count -eq 0) ("hours-named files in customer-setup: " + (($hoursNamed | ForEach-Object Name) -join ','))

$hook = 'https://bob\.ntsa\.uk/bob/v1/hours'
$contract = 'jeeves/docs/webhooks\.md'
$rules = 'launcher/agent-rules\.md'

# Templates that get copied into a customer repo.
foreach ($t in @($files | Where-Object { $_.Name -like '*.template' -or $_.Name -eq 'AGENTS.snippet.md' })) {
    $s = [IO.File]::ReadAllText($t.FullName)
    Gate ("no-hours-json:" + $t.Name) ($s -notmatch '(?i)hours\.json') 'template does not mention a repo hours.json'
    Gate ("no-repo-hours-instr:" + $t.Name) ($s -notmatch '(?i)(record|report|log|write|store)\s+(your\s+)?hours\s+(in|to|into)\s+(this|the)\s+(repo|folder|file)') 'no report-hours-in-repo wording'
    Gate ("no-hours-columns:" + $t.Name) ($s -notmatch '(?im)^\s*(hours|duration(_minutes)?|tquant)\s*:' -and $s -notmatch '(?i)\|\s*hours\s*\|') 'no hours fields or columns'
}

$snip = Read 'launcher\customer-setup\AGENTS.snippet.md'
Gate 'snippet-webhook' ($snip -match $hook -and $snip -match $contract) 'snippet points at the Bob hours webhook contract'
Gate 'snippet-rules' ($snip -match $rules) 'snippet points at canonical rules'
Gate 'snippet-never-repo' ($snip -match '(?i)never store or report hours in this repo') 'snippet forbids repo hours'

$log = Read 'launcher\customer-setup\log.md.template'
Gate 'log-no-hours' ($log -match '(?i)not the hours\s+store' -and $log -match '(?i)do not write hours') 'log template is narrative only'

$proj = Read 'launcher\customer-setup\project.md.template'
Gate 'project-booking-meta' ($proj -match '(?m)^priority_project:' -and $proj -match '(?m)^wbs:') 'booking project and WBS stay in project.md'

$readme = Read 'launcher\customer-setup\README.md'
Gate 'readme-webhook' ($readme -match $hook -and $readme -match $contract) 'setup README points at webhook'
Gate 'readme-no-optional-hours-json' ($readme -notmatch '(?i)optional repo-root `?hours\.json') 'setup README does not offer hours.json'
Gate 'readme-never-seed' ($readme -match '(?i)never seed `hours\.json`') 'setup README forbids seeding hours.json'

$skill = Read '.grok\skills\priority-customer-setup\SKILL.md'
Gate 'skill-webhook' ($skill -match $hook -and $skill -match 'webhooks\.md') 'skill points at webhook contract'
Gate 'skill-never-repo' ($skill -match '(?i)NEVER stored or reported in a customer repo') 'skill forbids repo hours'
Gate 'skill-no-seed' ($skill -notmatch '(?i)(create|add|seed|copy)\s+(a\s+)?(repo-root\s+)?`?hours\.json`?\s+(with|from|containing)') 'skill does not instruct seeding hours.json'
$meta = Read '.grok\skills\priority-customer-setup\meta.json' | ConvertFrom-Json
Gate 'skill-version' ([version]$meta.version -ge [version]'1.1.0') ("version " + $meta.version)

$ar = Read 'launcher\agent-rules.md'
Gate 'rules-never-repo' ($ar -match '(?i)Hours never go in the customer repo') 'agent-rules forbids repo hours'
$rv = [int]([regex]::Match($ar, 'rules-version:\s*(\d+)').Groups[1].Value)
Gate 'rules-version' ($rv -ge 8) "rules-version $rv"

if ($failed -gt 0) { Write-Host "Test-CustomerSetupNoRepoHours FAIL ($failed)"; exit 1 }
Write-Host 'Test-CustomerSetupNoRepoHours PASS'
exit 0
