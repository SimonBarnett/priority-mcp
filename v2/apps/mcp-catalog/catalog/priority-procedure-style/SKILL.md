---
name: priority-procedure-style
description: >
  Write or edit Priority form triggers and SQLI procedures: required banners,
  Inputs/Outputs :VARs, debug includes, dedicated vs shared TRIG, '' vs '\0'
  strings, PROGPARAM/:$.NAME, GOTO vs LOOP. Use when authoring triggers,
  #INCLUDE bodies, or /priority-procedure-style.
---

# Priority procedure style

Foundation: harvest-priority-skills -> report back to https://github.com/SimonBarnett/priority-mcp.

Grab from catalog MCP `https://mcp-priority.ntsa.uk/mcp` (`get_skill` with `name=priority-procedure-style`).

Authority: `docs/skill-sources/programming/PROCEDURE_STYLE.md`.

## When

Authoring or heavily editing form triggers, SQLI procedures, or #INCLUDE bodies.

## Hard rules

1. Banner with name + one-line purpose; `/* Inputs */` / `/* Outputs */` listing `:VAR`s; Heading / Sub-heading sections.
1a. **Debug every step.** Each step of a new trigger writes `FORMAT ADDTO` to a `bin.95`-relative file (`'../../file'` in the Priority root). Tag the step. Do not gate it off and do not use a path outside the Priority tree. See `PROCEDURE_STYLE.md`.
2. Update headers on existing stack triggers you touch.
3. Debug: site debug `#INCLUDE`; set `:RUN_BY` before includes; gate on `:DEBUG=1` → `:DEBUGFILE`. Field-wipe probes: **priority-format-addto-debug** (`SELECT 'tag',:$.COL FROM DUMMY FORMAT ADDTO` on one TEXTLINE).
4. Prefer **dedicated** TRIG ids; do not overwrite shared POST-FORM used by siblings.
5. Customer prefix; copy vendor — do not edit vendor in place.
6. **One type per `:VAR` name.** Form Prep error `Variable with two different types : X` means `:X` was used as CHAR and INT/REAL, or as INT and REAL, in the same compile unit. `:QTY = 0` is INT; `SUM` of INT `QUANT` stays INT; a REAL column types the same name REAL. Use `:QTY = 0.0` and `SELECT 0.0 + SUM(QUANT) INTO :QTY`. Rename CHAR clashes (e.g. `:DWFIXSORT` vs numeric `:SORT`). See `FORMTRIG_VAR_TYPES.md`.
7. **PRE-FORM is not TRIG -1.** On a current dictionary TRIG -1 is CHOOSE-FIELD and TRIG 7 is PRE-FORM. Read `TRIGGERS.TRIGNAME` before inserting `FORMTRIG`.
7a. **`:$.` is this form, `:$$.` is the parent, `:$$$.` is the grandparent.** Each extra `$` is one level up. A subform reads the parent key as `:$$.COL`, not `:$.COL`.
8. **`FORMTRIGTEXT` / `PROGRAMSTEXT` max ~68 chars per line.** Split long SQL; truncated INTO lists break runtime after a clean Prepare.
9. **Multi-char strings use `''`, not `'\0'`.** `:VAR = '\0'` is a one-char boolean (Y/`\0`). Init and compare work-order / part / status strings with `''`.
10. **Declare PROGPARAM on every step that reads `:$.NAME`** (INPUT and SQLI). When converting INPUT to plain CHAR params, clear leftover INPUT `PROGRAMSTEXT` (old LINK/ERRMSG).
11. **`GOTO` forward only; `LOOP` for backward** jumps to an earlier LABEL.
12. **Report work tables:** prefer `T$TABLE` / `REPTABLES` without SQLI `LINK` / report `FILE` PROGPARAM. Orphan FILE params cause the system *file links* message (not your TRIGMSG).
13. When editing report expressions, clear **`REPCLMNSA.EXPR`** as well as `REPCLMNSTEXT`.

## Related

- Form Prep / generator nav: **priority-form-engineering**
- Msg 154 / `:$1` optimistic lock: `docs/skill-sources/programming/MSG154_OPTIMISTIC_LOCK.md`
- Custom work-order close that writes balance before the transition completes (exact edit unknown): `priority-service-desk-record-modified-custom-close`. Do not use the original-buffer resync as that repair.
- UDATE mint: **priority-sql-udate-user**
- Form Prep WCF: **priority-formprep**
- Report zooms: **priority-report-form-zooms**

## SQLI custom columns

Do not name custom queue/work columns `PART`/`SERIAL`/`SERN` — clash with entities/:VARs. Use `QPART`-style names. See `PROCEDURE_STYLE.md`.
