# Priority procedure / trigger style

Priority-generic. CE examples are labeled.

## Banner (required on new or heavily touched triggers/procedures)

```
/* <NAME> â€” <one-line purpose> */
/* Inputs */
/*   :VAR1 â€” â€¦ */
/* Outputs */
/*   :VAR2 â€” â€¦ */
/* Heading */
/* Sub-heading */
```

When editing existing stack triggers, bring headers up to this standard.

## Debug on every step (CAST IRON)

A new or rewritten trigger ships with a debug write on **every step**. Do not wait for a failure and do not gate the writes behind `:DEBUG = 0`.

The file path is relative to `bin.95`. `'../../file'` is a file in the Priority root. Do not write the debug file to an absolute path outside that tree.

```
:DBG = '../../file';
SELECT 'STEP', :VAR FROM DUMMY
FORMAT ADDTO :DBG ;
```

Tag each step (`'PART'`, `'DEL'`, `'LOOP'`, `'INS'`, `'EXIT'`). `SELECT` needs `FROM DUMMY`. `FORMAT ADDTO` may be the next TEXT line when the probe does not fit in 68 characters. Remove the probes only when the user says the diagnosis is finished, then prepare again.

Site `#INCLUDE` debug files remain available. They do not replace the per-step `FORMAT ADDTO` lines.

## FORMAT ADDTO FROM DUMMY (field wipe / chronological probes)

When a form field is silently cleared and site debug includes are unavailable,
probe with Priority `FORMAT ADDTO` and the **`DUMMY`** table:

```
SELECT 'tag',:$.LOCNAME FROM DUMMY FORMAT ADDTO '../../tmp/s.txt';
```

Whole statement on **one** TEXTLINE; classic `TEXT` is nvarchar(68). Split
SELECT / FORMAT → Form Prep error at `FORMAT`. No `FROM` → Form Prep rejects.
Full playbook: `FORMAT_ADDTO_DUMMY_DEBUG.md` / **priority-format-addto-debug**.

## Shared vs dedicated triggers

Overwriting a shared POST-FORM (one TRIG id used by many forms) silently changes siblings. Prefer a **dedicated** TRIG for feature-specific mint/history logic (CE PART long-desc: shared TRIG 1306 replaced with dedicated 3797).

## #INCLUDE navigation (Form Generator)

- F6 on include line â†’ INCLUDE Line; F12 then F6 for body.
- Do not F6 an empty Form Name (opens wrong generator).

## Customization rules

- Customer objects use a 4-letter prefix.
- Copy vendor objects; do not edit vendor in place.

## FORMTRIGTEXT.TEXT line length (CAST IRON)

Each `FORMTRIGTEXT.TEXT` row is short (observed **max length 68** on current
estates). Long `SELECT … INTO :$1.…` lines truncate mid-statement and leave
SELECT/INTO column lists mismatched — Form Prep may still succeed while
runtime is wrong.

Split across multiple TEXTORD lines. Count characters before INSERT/UPDATE.
See also msg 154 resync hygiene in `MSG154_OPTIMISTIC_LOCK.md`.

## Shifted quantity (CAST IRON)

`QUANT` / `TQUANT` are shifted integers: SQL `bigint`, dictionary `INT` width 17, precision 3. Stored `1000` displays as `1.000`. Sum them into a column of the same definition. Initialise with `:QTY = 0` and `SELECT SUM(QUANT) INTO :QTY`. Do not use `0.0` and do not divide by 1000. Detail: `SHIFTED_INTEGERS.md`.

## Form level prefixes (CAST IRON)

In a form trigger, the dollars before the dot select which form the column belongs to:

| Prefix | Form |
|--------|------|
| `:$.COL` | The form the trigger is on |
| `:$$.COL` | The parent form |
| `:$$$.COL` | The parent of the parent |

Each extra `$` walks one level up. A subform that needs the parent's key reads `:$$.PART` (or the parent's key column), not `:$.PART`. `:$.PART` is a column on the subform itself.

`FORMCLMNSA` join expressions use the same prefixes (`= :$$.COL`).

## :VAR type consistency (CAST IRON)

Priority Form Prep fails with:

`Variable with two different types : VARNAME`

when the same `:VAR` is used as CHAR in one place and INT/REAL in another
**in the compile unit** (same trigger, #INCLUDE chain, or forms prepared
together).

### Known CE case (2026-09-28)

`ZCLA_ELEDITSPLIT` TRIG 22 had `:SORT = ''` (CHAR) then
`INTO :SORT, :FIXID` from `STRIND(...)` while other CE triggers
(e.g. `ZCLA_FIXACT`) use `:SORT` as numeric (`:SORT = 0`,
`MAX(SORT) INTO :SORT`).

**Fix:** rename the CHAR use to a dedicated name, e.g. `:DWFIXSORT`.
Do not reuse a well-known numeric name for a string.

Replay SQL (DEV applied): see Day Works
`scripts/fixes/Fix-ZCLA_ELEDITSPLIT-SORT.SQL.

## Custom table / queue column names in SQLI (CAST IRON)

Do **not** name physical or dictionary columns on custom work/queue tables
`PART`, `SERIAL`, `SERN`, or other Priority **entity** names when those tables
are read/written from SQLI.

In SQLI, a bare `PART` in `SELECT` / `INSERT` column lists is easy to resolve
as the PART entity or the `:PART` variable instead of your table column. Symptom:
BFS/queue dequeue leaves the intended INT at 0 and a branch (e.g. BOM expand)
is skipped with no obvious compile error.

**Fix:** prefix custom columns (`QPART`, `QPQTY`, `BPART`, …). Keep entity
columns only on real entity tables (`PART`, `SERIAL`, …).

Observed on a customer as-built tree fill (2026-11-03): queue column `PART`
broke PARTARC BOM fallback until renamed to `QPART`.

## String init: `''` vs `'\0'` (CAST IRON)

In Priority SQLI / triggers, **`:VAR = '\0'` defines a one-character boolean
flag** (the Y / `\0` pair). It does **not** mean “empty string”.

- Multi-character strings (`:WOIN`, `:PARTNAME`, `:STATUS`, …) must be
  initialised and compared with **`''`** (empty string), e.g.
  `:WOIN = '';` and `GOTO 90001 WHERE :WOIN = '';`.
- Keep `'\0'` only for true one-char flags.
- Mid-procedure resets of multi-char vars must also use `''`, not `'\0'`.

Symptom when wrong: interface input appears empty, `ERRMSG` on “missing”
lookup, or only the first character of a work-order / part name is kept.

## Interface PROGPARAM and `:$.NAME`

- Declare each interface name the step reads on **that step’s** `PROGPARAM`
  (INPUT and any SQLI that does `SELECT :$.WO INTO …`).
- INPUT-only PROGPARAM is not always enough for a later SQLI step to receive
  `:$.WO` / `:$.SER`.
- When converting an INPUT step from LINK/FILE params to plain CHAR params,
  **delete leftover `PROGRAMSTEXT`** on the INPUT PROG. Old `LINK …` /
  `ERRMSG` lines keep running and can fire before SQLI. Estate pattern for a
  param-only INPUT: PROGPARAM rows, **zero** PROGRAMSTEXT lines.

## GOTO vs LOOP (CAST IRON)

- **`GOTO` resolves forward LABELs only** (exits / skip-ahead).
- **Backward** jumps to an earlier `LABEL` must use **`LOOP`** (optionally
  `LOOP n WHERE …`).
- Using `GOTO` for a back-edge often surfaces as Prepare: *GOTO command with
  no corresponding LABEL* (message may truncate the procedure name).
- Prefer high numeric LABEL / GOTO / LOOP ids (1000+, 50000+) so prep errors
  are not confused with procedure step numbers (10 / 20 / 30).

## Report work tables: no FILE / no LINK (common pattern)

For SQLI → custom work table → report (`T$TABLE` / `REPTABLES`):

- Do **not** `LINK` the work table to a report `FILE` PROGPARAM unless the
  report step truly consumes that file buffer.
- A report PROGPARAM with `TYPE=FILE` and no matching SQLI `LINK` yields the
  system message *There is a problem with the file links. Contact your…*
  (this is **not** your `TRIGMSG` / `ERRMSG n` text).
- Prefer: SQLI `DELETE`/`INSERT` the physical table; report reads via
  `T$TABLE` + `REPTABLES` only (same shape as many as-built / stack reports).

## Report column expressions: clear `REPCLMNSA.EXPR` too

When replacing or removing an expression column on a report:

1. Delete `REPCLMNSTEXT` for that `T$EXEC` / POS.
2. Also delete or clear **`REPCLMNSA.EXPR`** for that POS — Form Generator
   stores the first expression line there. Clearing TEXT alone can leave a
   poisoned prepare (`parse error at or near token (`) when the compiled
   WHERE concatenates column + leftover EXPR).

## Deploying `PROGRAMSTEXT` with empty-string literals via T-SQL

Updating `PROGRAMSTEXT.TEXT` to values that contain Priority `''` is easy to
mangle in T-SQL quoting (`N':WOIN = '''';';'` and similar). Prefer:

```sql
DECLARE @Empty nvarchar(4) = CHAR(39) + CHAR(39); -- ''
UPDATE PROGRAMSTEXT
SET TEXT = N':WOIN = ' + @Empty + N';'
WHERE PROG = @Prog AND TEXTLINE = @Line;
```

Keep each TEXT line ≤ **68** characters (same limit as FORMTRIGTEXT).
