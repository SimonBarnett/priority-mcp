# Feature request: Harvest → priority-mcp via bob intake

**Repo:** `SimonBarnett/priority-mcp`  
**ID:** `PRI-04`  
**Phase:** `P4`  
**Date:** 2026-11-21  
**Theme:** Bob ear systray Agents/Workers/Weights + models webhook  
**Index:** `docs/feature-request-priority-agents-menu-index-2026-11-21.md`  
**Depends-on:** (none)  
**Skill:** visionary  
**Park:** park-only (no bob-job-loop)

## Objective

Deliver exactly one acceptance slice: **Harvest → priority-mcp via bob intake**.

LOCKED

## Success

| id | metric | target | how measured | fail-when |
|----|--------|--------|--------------|-----------|
| S1 | Harvest → priority-mcp | Intake path enforced | AGENTS/skill cites SimonBarnett/priority-mcp | Wrong harvest repo |

LOCKED

## Shape

Primary: app

Hybrid note: anti-omnibus slice of theme `Bob ear systray Agents/Workers/Weights + models webhook`; reuse existing SimonBarnett/priority-mcp product surface.

LOCKED

## Stack

Default: existing SimonBarnett/priority-mcp stack (Windows fleet, public GitHub, bob intake). This slice does not invent SaaS hosts or secrets.

Why: smallest shippable FR for anti-omnibus park.

Why-not: omnibus tray mega-issue.

LOCKED

## Architecture

```
theme brief -> PRI-04 slice -> SimonBarnett/priority-mcp change surface
depends: (none)
```

LOCKED

## Screens

HTML mocks (shared theme folder or local). Validator uses `--mocks-dir`.

| id | file | state |
|----|------|-------|
| M1 | home.html | primary |
| M2 | empty.html | empty |
| M3 | error.html | error |

## Gap vs current tree

See theme index. This issue owns only S1 above.

## Out of scope

Every other backlog ID. Implementation is after park when dispatched.

## LOCKED

- One acceptance row only
- Park-only until human dispatch
- Cross-link index + depends-on

## UNKNOWN

- Exact initial models matrix seed values (BOB-04) until CLI `--help` / Simon seed
- Favicon ToS for third-party sites when applicable
