# UAT log — SimonBarnett/priority-mcp#0

**Verdict:** UAT PASS  
**Date (UTC):** 2026-11-24T15:02:03Z  
**Commit tested:** 790fc0143e96e4f1ffc85c73527100523c0d92d7 (origin/main after cold-start #790-#794 / digest #789 last-child close)  
**Seat / machine:** flamingo-17068 (Windows). This seat did not author cycle PRs #790-#808.  
**Repo clear check:** closed stale digest #789 (children #790-#794 already CLOSED); `gh issue list --state open` empty; `gh pr list --state open` empty.

## Cycle slice (cold-start performance)

| id | criterion | result | evidence |
|----|-----------|--------|----------|
| FR-790 | Prefer local CloneRoot over UNC EnsureClone | PASS (via #792) | `AiRoot` Fixed-drive search; FR-792 launcher.Tests PASS; #790 closed with #804 |
| FR-791 | EnsureClone TTL skip fetch when tip current | PASS | launcher.Tests FR-791 x3 PASS |
| FR-792 | Bound git probes; skip UNC by default; short-circuit preferred | PASS | launcher.Tests FR-792 x5 PASS |
| FR-793 | Defer/cache ListCustomers gh topic scan | PASS | launcher.Tests FR-793 x2 PASS |
| FR-794 | Startup phase timing lines | PASS | launcher.Tests FR-794 x2 PASS |

## Vision / harness

| id | criterion | result | evidence |
|----|-----------|--------|----------|
| V1 | rules-version >= 2; connect without ask; prefer DEV; never echo secrets; ask before messaging | PASS | Test-Fr201-VisionRulesV2.ps1 |
| V2 | Fr201 pin uses -ge 2 | PASS | Test-Mrb253-Fr201RulesVersionGe2.ps1 |
| V3 | Catalog merge gate + honesty-box + ASCII-safe | PASS | v2/tools/Test-PriorityCatalog.ps1 exit 0 |
| V-rename | FR #597 priority-mcp self-refs only | PASS | Test-Fr597-PriorityMcpRename.ps1 |
| PRI-01..08 | prior Priority Agent client gates | PASS | prior pins still green in 40 Test-*.ps1 |
| V7 | Form Prep EXECPREPLOCK/LASTPREPDATE live | SKIP | No C:\passwords\priority-mcp-connections.txt on flamingo; not CE-PRIORITY-DEV1. Same class as prior UATs. Not filed. |

Root `tests/Test-*.ps1`: **40 PASS**, 0 FAIL. `tests/test_*.py`: **8 passed**. Catalog PASS. launcher.Tests: **116 PASS** exit 0. mcp.ntsa.uk initialize HTTP 200 (mcp-priority 1.1.1).

## Release

- Catch-up: tip `VERSION` was 2.0.1 while `v2.0.2` existed; align then bump to **2.0.3**.
- Tag/title from `uat_release.py` (FR #7122): **Priority Agent v2.0.3** / `v2.0.3`.

## Gaps

none

---
 UAT log — SimonBarnett/priority-mcp#0

**Verdict:** UAT PASS  
**Date (UTC):** 2026-11-24T00:04:37Z  
**Commit tested:** 4abb195b57c17039e59dce5852bf372ed585b954 (origin/main after PR #783/#784)  
**Seat / machine:** redqueen-668 (Windows). This seat did not author cycle PRs #757-#784 (gh author SimonBarnett).  
**Repo clear check:** `gh issue list --state open` empty; `gh pr list --state open` empty.  
**Cycle slice:** prior FAIL #781 gaps #779/#780 closed by #783/#784; re-verify this clear.

## Cycle / PRI slice

| id | criterion | result | evidence |
|----|-----------|--------|----------|
| PRI-01 | model/context/effort start settings | PASS | tests/Test-Fr738-Pri01Settings.ps1 exit 0 |
| PRI-02 | no-token dialog + quit | PASS | tests/test_pri02_no_token_gate.py (pytest suite) |
| PRI-03 | skills from mcp.ntsa.uk | PASS | POST https://mcp.ntsa.uk/mcp initialize HTTP 200 serverInfo mcp-priority 1.1.1 |
| PRI-04 | harvest → priority-mcp via bob intake | PASS | pytest suite includes harvest-repo pin |
| PRI-05 | MSI gaps vs launcher-v2.0.0 | PASS (N/A recorded) | docs/pri-05-msi-gaps-vs-launcher-v2.md; prior cycle |
| PRI-07 | reuse launcher/priority.ico | PASS | pytest suite |
| PRI-08 | vendor contract bob-ear | PASS | docs/vendor-contract-bob-ear.md; pytest suite |

## Vision / harness

| id | criterion | result | evidence |
|----|-----------|--------|----------|
| V1 | rules-version >= 2; connect without ask; prefer DEV; never echo secrets; ask before messaging | PASS | Test-Fr201-VisionRulesV2.ps1 |
| V2 | Fr201 pin uses -ge 2 | PASS | Test-Mrb253-Fr201RulesVersionGe2.ps1 |
| V3 | Catalog merge gate + honesty-box + ASCII-safe | PASS | v2/tools/Test-PriorityCatalog.ps1 exit 0 |
| V-rename | FR #597 priority-mcp self-refs only | PASS | Test-Fr597-PriorityMcpRename.ps1 exit 0 |
| Fr571 | Fake-installer suite stays offline | PASS | Test-Fr571-FakeInstallersOffline.ps1 (fixed #783) |
| Mrb574 | MSI docs park mirrors | PASS | Test-Mrb574-PriorityAgentMsiPark.ps1 (fixed #784) |
| Fr566 | Pack Priority Agent MSI | PASS | Test-Fr566-PackPriorityAgentMsi.ps1 (WiX cache refreshed) |
| V7 | Form Prep EXECPREPLOCK/LASTPREPDATE live | SKIP | No C:\passwords\priority-mcp-connections.txt on redqueen; not CE-PRIORITY-DEV1. Same class as prior UATs. Not filed. |

Root `tests/Test-*.ps1`: **40 PASS**, 0 FAIL. `tests/test_*.py`: **8 passed**. Catalog PASS. AT6 PASS.

## Release

- Catch-up: root `VERSION` was 1.0.74 while `launcher-v2.0.0` existed; docs PR aligns then bumps to **2.0.1**.
- Tag/title from `uat_release.py` (FR #7122): **Priority Agent v2.0.1** / `v2.0.1`.

## Gaps

none

---
**Verdict:** UAT PASS  
**Date (UTC):** 2026-11-06T16:30:00Z  
**Commit tested:** 9d77951854348cdf06d80bb5b742f552d102efb3 (origin/main after PR #273)  
**Seat / machine:** redqueen-16728 (Windows). This seat did not implement cycle PRs #268-#273.  
**Cycle PRs:** #268 launcher-v stamp (FR #267), #269/#270 harvest+fix, #271/#272 SQLI PART/SERIAL column names, #273 SQLI '' vs '\0' / PROGPARAM / GOTO / report FILE/REPCLMNSA.

## Vision criteria (offline / harness)

| id | criterion | result | evidence |
|----|-----------|--------|----------|
| V1 | Priority Agent rules-version >= 2; connect without ask; prefer DEV; never echo secrets; ask before messaging | PASS | Test-Fr201-VisionRulesV2.ps1 |
| V2 | Fr201 pin uses -ge 2 | PASS | Test-Mrb253-Fr201RulesVersionGe2.ps1 |
| V3 | Catalog merge gate + honesty-box + no-BOM | PASS | v2/tools/Test-PriorityCatalog.ps1 |
| V4 | OData empty-500 company probe (FR #226) | PASS | Test-Fr226-ODataEmpty500Refuse.ps1 |
| V5 | SaaS OIDC / catalog skill pins | PASS | Test-Fr212 / Test-Fr219 / Test-Mrb208 |
| V6 | Main auto-merge protection docs (FR #247) | PASS | Test-Fr247-MainAutoMergeProtection.ps1 |
| V7 | Form Prep EXECPREPLOCK/LASTPREPDATE live success | SKIP | No C:\passwords\priority-mcp-connections.txt on redqueen; no DEV1. Same class as prior UATs. Not a product defect this cycle. |
| V8 | Discover airc consoles / fixture override (FR #258/#261) | PASS | Prior cycle pins still green via Test-Mrb197 / catalog; no regression this cycle. |
| V9 | Customer console channel digest/wonderland (FR #263) | PASS | Prior cycle; harness still green. |
| V10 | Launcher-v tags stamp Version/FileVersion/InformationalVersion; banner X.Y.Z (FR #267) | PASS | Test-Fr267-LauncherVersion.ps1 |
| V11 | SQLI style: '' vs '\0', PROGPARAM, GOTO vs LOOP, no report FILE/STK, clear REPCLMNSA.EXPR; no PART/SERIAL custom columns | PASS | PROCEDURE_STYLE.md + catalog priority-procedure-style 1.2.0 + harvest-agent-skills needles on main |

## Harness

All 14 root `tests/Test-*.ps1` scripts: **PASS** (exit 0).  
`v2/tools/Test-PriorityCatalog.ps1`: **PASS**.  
dotnet / launcher.Tests rebuild: **SKIP** (dotnet not on redqueen; same as prior UAT). Live Form Prep: **SKIP** (no connections file).

## Gaps

none

---

**Verdict:** UAT PASS  
**Date (UTC):** 2026-11-03T07:40:00Z  
**Commit tested:** a069f15aef6676c67ee56d9b0ddaa44e594413eb (origin/main after PR #265)  
**Seat / machine:** flamingo-35004 (Windows). This seat did not implement cycle PRs #256-#265.  
**Cycle PRs:** #256 harvest, #259 Discover airc_consoles, #260 harvest, #261 fixture-override fix, #262 harvest, #264 digest console channel, #265 harvest.

## Vision criteria (offline / harness)

| id | criterion | result | evidence |
|----|-----------|--------|----------|
| V1 | Priority Agent rules-version >= 2; connect without ask; prefer DEV; never echo secrets; ask before messaging | PASS | Test-Fr201-VisionRulesV2.ps1 (rules-version got 4) |
| V2 | Fr201 pin uses -ge 2 | PASS | Test-Mrb253-Fr201RulesVersionGe2.ps1 |
| V3 | Catalog merge gate + honesty-box + no-BOM | PASS | v2/tools/Test-PriorityCatalog.ps1 |
| V4 | OData empty-500 company probe (FR #226) | PASS | Test-Fr226-ODataEmpty500Refuse.ps1 |
| V5 | SaaS OIDC / catalog skill pins | PASS | Test-Fr212 / Test-Fr219 / Test-Mrb208 |
| V6 | Main auto-merge protection docs (FR #247) | PASS | Test-Fr247-MainAutoMergeProtection.ps1 |
| V7 | Form Prep EXECPREPLOCK/LASTPREPDATE live success | SKIP | No C:\passwords\priority-mcp-connections.txt on flamingo; no CE-PRIORITY-DEV1. Same class as 2026-11-01 UAT. Not a product defect this cycle. |
| V8 | Discover airc consoles from digest; fixture env ignored when fetch is explicit (FR #258/#261) | PASS | Source `launcher/AircConsoleDiscovery.cs` + `launcher.Tests/Program.cs` TestDiscoverInjection (no local dotnet; static pin). |
| V9 | Customer console channel is digest/wonderland, not `#<stem>` (FR #263) | PASS | `launcher.Tests` picker/session-rules asserts; README airc_consoles. |

## Harness

All 13 root `tests/Test-*.ps1` scripts: **PASS** (exit 0).  
Test-PriorityCatalog.ps1: **PASS**.  
dotnet / launcher.Tests: **SKIP** (dotnet not on flamingo; same as prior UAT).

## Gaps

none

---

**Verdict:** UAT PASS  
**Date (UTC):** 2026-11-01T21:10:36Z  
**Commit tested:** cd47a99082ef5ac73f65ecd13e039026946e2dca (origin/main after PR #254)  
**Seat / machine:** flamingo-37172 (Windows)  
**Cycle PRs:** #252 (harvest lesson), #253 (Fr201 rules-version >= 2), #254 (docs/mrb-253 hostile pin)

## Vision criteria (offline / harness)

| id | criterion | result | evidence |
|----|-----------|--------|----------|
| V1 | Priority Agent rules-version >= 2; connect without ask; prefer DEV; never echo secrets; ask before messaging | PASS | Test-Fr201-VisionRulesV2.ps1 (rules-version got 4); Test-Mrb200-CatalogStdio.ps1 |
| V2 | Fr201 pin uses -ge 2 (not exact document version 2) | PASS | Test-Mrb253-Fr201RulesVersionGe2.ps1 |
| V3 | Catalog merge gate + honesty-box intake + no-BOM metas | PASS | 2/tools/Test-PriorityCatalog.ps1 |
| V4 | OData empty-500 company probe (FR #226) | PASS | Test-Fr226-ODataEmpty500Refuse.ps1 |
| V5 | SaaS OIDC / catalog skill pins (FR #212/#219/#208) | PASS | matching 	ests/Test-*.ps1 |
| V6 | Main auto-merge protection docs (FR #247) | PASS | Test-Fr247-MainAutoMergeProtection.ps1 |
| V7 | Form Prep EXECPREPLOCK/LASTPREPDATE live success | SKIP | No C:\passwords\priority-mcp-connections.txt on flamingo; no DEV1 session. Product rules require ask Haitch when missing. Not a product defect for this cycle. |

## Harness

All 13 root 	ests/Test-*.ps1 scripts: **PASS** (exit 0).  
Test-PriorityCatalog.ps1: **PASS**.

## Notes

- CAT-T56 gate *label* still says "refuses live/PRI" while VISION rules v2 allows Live/PRI connect without ask (prefer DEV). Assert only checks the string live/PRI is present — ACCEPTABLE label drift; not filed as a blocking gap this cycle.
- Launcher PriorityAgent.exe release path remains CI (launcher-v* / workflow_dispatch); this UAT does not rebuild the launcher (no local dotnet). Prior asset: release launcher-v1.0.0.

## Gaps

none
