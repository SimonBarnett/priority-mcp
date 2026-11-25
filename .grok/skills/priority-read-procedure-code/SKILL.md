---
name: priority-read-procedure-code
description: >-
  Read Priority procedure, form, report, and interface code from the system
  database. PROGRAMSTEXT, FORMTRIGTEXT, REPCLMNSTEXT, and LOADTEXT.
  Prep files are binary. Use when copying or reading Priority code.
---

# Read Priority code

Code text lives in the **`system`** database, not the company database. `system\prep\d{id}.prp` is a binary prep file, not the source. `dbo.[T$EXEC]` holds `ENAME`, `TYPE`, `TITLE`, and `T$EXEC`.

On the AIRC PowerShell shell, escape `$` as `` `$ ``. Keep a command under 400 bytes. A reply is cut at about 48 lines. Write a long result to a file on that host and read it back in short pieces.

## Procedures

`dbo.PROGRAMS` is one row per step: `T$EXEC`, `POS`, `PROG`. `PROG` is the step id. It is not `T$EXEC`. `dbo.PROGRAMSTEXT` is `PROG`, `TEXT`, `TEXTLINE`, `TEXTORD`. The SQL line is `TEXT`. Order by `TEXTLINE`. A step can have `PROGRAMS` and no `PROGRAMSTEXT`. No `PROGRAMS` rows means no SQL body. Only `TYPE` `P` uses `PROGRAMS`.

```sql
SELECT p.POS, p.PROG, t.TEXTLINE, t.TEXT
FROM dbo.PROGRAMS p
JOIN dbo.[T$EXEC] e ON e.[T$EXEC] = p.[T$EXEC]
LEFT JOIN dbo.PROGRAMSTEXT t ON t.PROG = p.PROG
WHERE e.ENAME = '<ENAME>'
ORDER BY p.POS, t.TEXTLINE;
```

## Forms

`dbo.FORMTRIG` is `FORM`, `TRIG`. `FORM` is the form's `T$EXEC`. `TRIG` joins `dbo.TRIGGERS` (`TRIG`, `TRIGNAME`). The body is `dbo.FORMTRIGTEXT`: `FORM`, `TRIG`, `TEXT`, `TEXTLINE`, `TEXTORD`, `INCFORM`. Order by `TEXTLINE`.

```sql
SELECT g.TRIGNAME, t.TEXTLINE, t.TEXT
FROM dbo.FORMTRIGTEXT t
JOIN dbo.[T$EXEC] e ON e.[T$EXEC] = t.FORM
JOIN dbo.TRIGGERS g ON g.TRIG = t.TRIG
WHERE e.ENAME = '<ENAME>'
ORDER BY g.TRIGNAME, t.TEXTLINE;
```

## Reports

Report column text is `dbo.REPCLMNSTEXT`: `T$EXEC`, `POS`, `TEXT`, `TEXTLINE`, `TEXTORD`. Order by `POS`, `TEXTLINE`. A report is not in `PROGRAMS`.

```sql
SELECT t.POS, t.TEXTLINE, t.TEXT
FROM dbo.REPCLMNSTEXT t
JOIN dbo.[T$EXEC] e ON e.[T$EXEC] = t.[T$EXEC]
WHERE e.ENAME = '<ENAME>'
ORDER BY t.POS, t.TEXTLINE;
```

## Interfaces

An interface is `dbo.[T$LOAD]`: `T$LOAD`, `FILENAME`, `TITLE`, `TYPE`, `REPORT`, `T$TABLE`. The body is `dbo.LOADTEXT`: `T$LOAD`, `TEXT`, `TEXTLINE`, `TEXTORD`. Order by `TEXTLINE`.

```sql
SELECT t.TEXTLINE, t.TEXT
FROM dbo.LOADTEXT t
JOIN dbo.[T$LOAD] l ON l.[T$LOAD] = t.[T$LOAD]
WHERE l.FILENAME = '<FILENAME>'
ORDER BY t.TEXTLINE;
```

