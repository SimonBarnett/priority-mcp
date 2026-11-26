# Form Prep shadow tables and success gates

## Shadow tables

Before preparing forms that touch new tables, create in **pritempdb**:

- `dbo.T$$<TNAME>` mirroring live base columns
- `T$LINKID` as required by Form Prep temp linking
- Unique indexes on `T$$` tables with **`IGNORE_DUP_KEY = ON`** (same Tabula rule as live tables)

Also register dictionary objects (CATALOG family). `TNAME` max length **20**. Missing shadows → SQL 208 during prep.

## Success gate (never fake)

Success only when:

1. Runner/UI reports ok / completed, **and**
2. `EXECPREPLOCK.UPD = N`, **and**
3. `LASTPREPDATE` advanced for that form.

Never SQL-flip `UPD=N`.

## Paths

| Path | Notes |
|------|-------|
| Named Form Prep (`Prepare-NamedForm.ps1` / plugin `prepare_form`) | Preferred on allowlisted DEV; Web SDK EFORM → FORMPREPDRCT2 |
| Interactive WINRUN FORMPREP | `WINACTIV -P FORMPREP [FormName]`; no `-nbg`, no `-T` |
| Agent WINRUN | Often exits 0 with no EXECPREPLOCK change if other winactiv busy / non-interactive |
| `bin\formprep.exe` | Often "Can't connect to SQL Server - authorization failed" (no Integrated Security) — avoid |
| Console `PRIORITY prepare <ENAME>` | Prepares TYPE=P (and sometimes TYPE=R). TYPE=F returns `reason=not_type_p`. Use Form Generator or `formprep.exe -g <ENAME>` from an interactive desktop. |

## Parent `.crs` access denied (5)

Preparing a subform rewrites the parent compiled file `system\prep\d{parent T$EXEC}.crs` (the number is the parent's `T$EXEC`, not the child's).

```
Can't create file <PriorityRoot>/system/prep/d<parent>.crs access is denied (5)
```

Typical cause: `formprep.exe -g <child>` runs as a Windows user who is in Administrators, but the token is not elevated. `system\prep` gives Users read and execute. Container-only write bits do not allow creating or replacing a file in that directory. An error dialog that is already open does not see a later ACL change.

Fix: grant that Windows user Modify, object-inherit and container-inherit, on `system\prep` (propagate to existing files), then start the prepare again. Do not flip `EXECPREPLOCK.UPD` in SQL. Session 0 / LocalSystem still does not complete Form Prep.

## Environment

Si may lack API licence on some TEST hosts — use client Form Prep there; leave `UPD=Y` honest until prepared.
