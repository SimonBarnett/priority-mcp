# Read procedure step text via EPROG subforms (SaaS WCF)

Foundation: harvest-priority-skills -> report back to https://github.com/SimonBarnett/priority-mcp.

Harvested 2026-11-18 from a SaaS Live read of a custom procedure (customer details stripped).

## Problem

`formStart('PROG')` (Procedure Steps) may return **The form is unprepared** on SaaS even after a Form Prep walk reports success. `proc text` on EXEC only reads header `TEXT`, which is often empty for multi-step procedures.

## Working read path (read-only)

1. `formStart` **EPROG** (Procedure Generator; often REST/WCF ready).
2. `setSearchFilter` on `ENAME` = procedure internal name.
3. `getRows` (expect one row).
4. `startSubForm('PROG')` — Procedure Steps for that procedure.
5. For each step row (`POS`, `ENAME` such as `SQLI` / `INPUT`):
   - Activate the step row when the SDK requires it.
   - `startSubForm('PROGTEXT')` and read `TEXT` (concatenate continuation rows if the SDK returns multiple).

Do **not** invent subform names. If `startSubForm('PROG')` fails with “Can't find Sub Form”, stop and confirm the parent is EPROG with a retrieved procedure row.

## Related

- `priority-saas-wcf` `proc text` (FR #696): EXEC header TEXT first; automatic EPROG→PROG→PROGTEXT fallback with `steps[]` when header TEXT is empty.
- WINHTML file output paths: prefer Priority-root relative paths; for user DOWNLOAD / mail delivery, `../../system/mail/<file>` is the usual attachable location (see SDK document-execution examples using `../../` and mail folders under `system/mail`).
