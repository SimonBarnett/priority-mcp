---
name: priority-format-addto-debug
description: >
  Debug Priority form column triggers with FORMAT ADDTO FROM DUMMY: one
  TEXTLINE probes, nvarchar(68) classic limit, chronological wipe location.
  Use when a form field is silently cleared (e.g. LOCNAME to 0), or
  /priority-format-addto-debug.
---

# FORMAT ADDTO FROM DUMMY (form trigger debug)

Foundation: harvest-priority-skills -> report back to https://github.com/SimonBarnett/priority-mcp.

Authority: `docs/skill-sources/programming/FORMAT_ADDTO_DUMMY_DEBUG.md`.

## When

A form field changes (or clears) during POST / CHECK / `#INCLUDE` and you need
proof of **which step** did it.

## Hard rules

1. **`FROM DUMMY`** — Priority SELECT needs a FROM; use `DUMMY` to dump `:$.`
   form variables.
2. **Path is relative to `bin.95`.** `'../../file'` lands in the Priority
   root. Do not use an absolute path outside that tree.
3. **≤ 68 characters** on classic `TEXT` (nvarchar(68)); Msg 8152 if longer.
   One line when it fits: `SELECT 'tag',:$.COL FROM DUMMY FORMAT ADDTO '../../tmp/s.txt';`
   When it does not, put the path in a CHAR variable and continue:
   `:DBG = '../../USAGE.DBG';` then `SELECT :$.COL, :$$.COL FROM DUMMY` /
   `FORMAT ADDTO :DBG ;`. The SELECT line must include `FROM DUMMY`.
4. Probe **before and after** each suspect POST / `#INCLUDE` / CHECK; read the
   ADDTO file in order; strip probes after diagnosis and Form Prep again.

## Canonical probe

```
SELECT 'tag',:$.LOCNAME FROM DUMMY FORMAT ADDTO '../../tmp/s.txt';
```

Typical land path: `E:\priority\tmp\s.txt` under a standard tree.

## Related wipe (warehouse bin)

After proving the wipe with ADDTO: re-assert stock bin after warehouse BUF
includes with a `BALANCE > 0` / `LOCNAME <> '0'` stock join. Avoid
`WHERE WARHS = :$.WARHS` alone when the form WAREHOUSES buffer binds bin `0`.
Details in the authority doc.

## Related skills

- **priority-procedure-style** — banners, 68-char lines, debug includes
- **priority-form-prep-after-sql-change** — Form Prep after trigger SQL
- **priority-formprep** — named Form Prep success gate
