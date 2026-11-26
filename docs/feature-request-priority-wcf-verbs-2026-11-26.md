# Feature request: Priority deterministic WCF verbs (thin CLI + dual transport)

**Target repo:** SimonBarnett/priority-mcp (Parent Digest + CLI source of truth)  
**Vendored into:** SimonBarnett/bobiverse (`airc/priority-worker`)  
**Supersedes / closes:** [bobiverse#6026](https://github.com/SimonBarnett/bobiverse/issues/6026)  
**Depends-on (airc relay only):** [bobiverse#9128](https://github.com/SimonBarnett/bobiverse/issues/9128) AIRC-HIDE  
**Note:** `agentic_fomprep` is deprecated / renamed to `priority-mcp`. Do not file new work under agentic_fomprep.

## Filed issues

| Repo | id | issue | title |
|---|---|---|---|
| priority-mcp | Parent | _(fill after file)_ | Digest: Priority deterministic WCF verbs (thin CLI + dual transport) |
| priority-mcp | D0 | _(fill after file)_ | Digest: Shared thin WCF CLI package + verb contract |
| priority-mcp | FR-A | _(fill after file)_ | Verb contract doc + JSON Schema + C:\passwords secret paths |
| priority-mcp | FR-B | _(fill after file)_ | CLI auth mode password (tabula) alongside Bearer/PAT |

Unfiled drafts (nested Digests D1–D6 and their FRs) stay in Plan work `plan-20261126-192700` until the next turn — anti-omnibus.

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

| id | Home | Title | Depends-on |
|---|---|---|---|
| D0 | priority-mcp | Shared thin WCF CLI + verb contract | — |
| D1 | priority-mcp | Prepare family | D0 |
| D2 | priority-mcp | Smoke open/run | D1 |
| D3 | priority-mcp | Create/edit without MSSQL | D0 |
| D4 | priority-mcp | Shell recording in DEV loop | D1, D3 |
| D5 | bobiverse | airc relay vendor transport | D0, D1, **#9128** |
| D6 | priority-mcp | MCP tools bind to CLI | D0, D1, D2 |

## Out of scope (this park)

Implementing CLI changes, hotpatch, live prepare, merges, filing D1–D6 in this turn.

## Screens / mocks

None (no product UI).
