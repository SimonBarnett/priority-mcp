# Feature request: Priority deterministic WCF verbs (thin CLI + dual transport)

**Target repo:** SimonBarnett/priority-mcp (Parent Digest + CLI source of truth)  
**Vendored into:** SimonBarnett/bobiverse (`airc/priority-worker`)  
**Supersedes / closes:** [bobiverse#6026](https://github.com/SimonBarnett/bobiverse/issues/6026)  
**Depends-on (airc relay only):** [bobiverse#9128](https://github.com/SimonBarnett/bobiverse/issues/9128) AIRC-HIDE  
**Note:** `agentic_fomprep` is deprecated / renamed to `priority-mcp`. Do not file new work under agentic_fomprep.

## Filed issues

| Repo | id | issue | title |
|---|---|---|---|
| priority-mcp | Parent | #834 | Digest: Priority deterministic WCF verbs (thin CLI + dual transport) |
| priority-mcp | D0 | #835 | Digest: Shared thin WCF CLI package + verb contract |
| priority-mcp | D1 | #838 | Digest: Prepare family (form|proc|report|interface) via CLI |
| priority-mcp | D2 | #839 | Digest: Smoke open/run (form|proc|report) |
| priority-mcp | D3 | #840 | Digest: Entity create/edit without MSSQL |
| priority-mcp | D4 | #841 | Digest: Shell recording in the DEV loop |
| bobiverse | D5 | #10025 | Digest: airc relay transport (vendors CLI); Depends-on #9128 |
| priority-mcp | D6 | #842 | Digest: MCP tools bind to in-repo CLI |
| priority-mcp | FR-A | #836 | Verb contract doc + JSON Schema + C:\passwords secret paths |
| priority-mcp | FR-B | #837 | CLI auth mode password (tabula) alongside Bearer/PAT |
| priority-mcp | docs PR | #833 | Park vision + FR index (merged) |

Child FRs under D1–D6 remain unfiled until each digest is sliced (anti-omnibus).

## Ultimate objective

An agent can run **create → prepare → open/smoke → shell** through one deterministic verb CLI over HTTPS (SaaS) or airc relay (on-prem), with prepare success/errors as structured JSON and no LLM tokens on the tooling path.

## Success

| id | metric | target | how measured | fail-when |
|---|---|---|---|---|
| S1 | One CLI | Source in priority-mcp; bobiverse vendors same pin | version/tag == airc vendor pin | Two prepare implementations both "current" |
| S2 | Deterministic prepare | `prepare form\|procedure\|report\|interface --name X` → `{ok, errors[]}` | unit + live fixture | Agent guesses success from free text |
| S3 | Smoke after prepare | `form start` / `proc start` / `report run` on prepared entity | live Z* DEV | Prepare ok but open fails unstructured |
| S4 | DEV workflow | table → form → prepare → open → shell | scripted Z* test | Win client or ad-hoc SQL required |
| S5 | Dual transport | Same verb JSON SaaS direct + airc relay | fixture + one live each | Divergent codes/verbs; **D5 blocked on #9128** |
| S6 | Secrets | Only `C:\passwords` / CredMan; never argv/IRC/logs/git | redaction + path tests | Secret in reply, commit, or repo file |
| S7 | Optional Priority | airc without Priority → `priority_not_installed` | #5960 tests | airc fails to start |
| S8 | Close bobiverse#6026 | Closed when Parent + D0 filed here | issue state | Omnibus left open |

## Shape

LOCKED — **service** (thin CLI + transports). No product web UI.

## Stack

- **CLI:** Node + `priority-web-sdk` in priority-mcp (`v2/apps/priority-saas-wcf` → shared contract in D0)
- **Direct:** MCP / local Agent → CLI → WCF `Service.svc` (password \| PAT \| Bearer)
- **Relay:** bobiverse airc vendors pinned CLI; CredMan on airc host; PRIORITY verbs via bobear
- **Secrets:** `C:\passwords\` (outside git) + CredMan; never print or push passwords
- **Shell:** existing `priority-shell-compile` / install pins

## Architecture

```
  priority-mcp (SOURCE OF TRUTH)
   thin CLI: priority-web-sdk + allowlist + verb JSON
        |                          |
        | MCP / local Agent        | vendor pin (pack/MSI)
        v                          v
  Direct HTTPS (SaaS)      bobiverse airc/priority-worker
                           (AIRC_PRIORITY_OWNED=1)
        both paths -> Priority WCF Service.svc
```

## Gap vs current tree

| Area | Today | Gap |
|---|---|---|
| CLI verbs | `priority-wcf.mjs` has prepare/form/proc/table/interface | Promote to dual-transport contract; password/tabula auth |
| airc | Python `priority_wcf.py` SOAP + WINRUN history | Vendor CLI; retire SOAP/WINRUN prepare default (D5, after #9128) |
| SaaS table create | `sqlProfile` / MSSQL gate | Web-SDK dictionary path (D3) |
| Shell in loop | compile/install skills exist | Wire create→prepare→smoke→shell (D4) |
| MCP bind | partial | Tools wrap CLI; agent FR only on tooling fail (D6) |

## Nested Digests (index)

| id | Home | Issue | Title | Depends-on |
|---|---|---|---|---|
| D0 | priority-mcp | #835 | Shared thin WCF CLI + verb contract | #834 |
| D1 | priority-mcp | #838 | Prepare family | #834 |
| D2 | priority-mcp | #839 | Smoke open/run | #834, #835 |
| D3 | priority-mcp | #840 | Create/edit without MSSQL | #834, #835 |
| D4 | priority-mcp | #841 | Shell recording in DEV loop | #834 |
| D5 | bobiverse | #10025 | airc relay vendor transport | priority-mcp#834/#835/#838, bobiverse#9128 |
| D6 | priority-mcp | #842 | MCP tools bind to CLI | #834, #835 |

## Out of scope (this park)

Implementing CLI changes, hotpatch, live prepare, merges, filing D1–D6 in this turn.

## Screens / mocks

None (no product UI).
