# FORMAT ADDTO FROM DUMMY — form trigger debug

Priority-generic. Use when a form column trigger silently changes a field
(for example a warehouse bin wiped to `0`) and you need chronological proof
of which POST / CHECK / `#INCLUDE` step did it.

## Why `DUMMY`

Priority `SELECT` always needs a `FROM` table. Form variables (`:$.COL`) are
not a table. **`DUMMY`** is the one-row system table for expressions and
variable dumps:

```
SELECT 'tag',:$.LOCNAME FROM DUMMY FORMAT ADDTO '../../tmp/s.txt';
```

Without `FROM DUMMY`, Form Prep rejects the statement (must SELECT FROM).

## Statement that Form Preps (classic)

Put the **whole** statement on **one** `FORMCLTRIGTEXT` / `FORMTRIGTEXT`
`TEXTLINE`. On classic installs `TEXT` is **nvarchar(68)** — longer inserts
fail with Msg 8152 truncation.

Working probe (fits 68 chars with a short tag):

```
SELECT 'tag',:$.LOCNAME FROM DUMMY FORMAT ADDTO '../../tmp/s.txt';
```

Relative path `../../tmp/s.txt` resolves under the Priority tree
(example: `E:\priority\tmp\s.txt`). Keep the path short so the line stays
within 68 characters.

## Failures

| Mistake | Symptom |
|---------|---------|
| `SELECT …` on one TEXTLINE, `FORMAT ADDTO …` on the next | Form Prep parse error at or near symbol `FORMAT` |
| `SELECT :$.X FORMAT ADDTO …` with no `FROM` | Form Prep rejects (needs FROM; use `DUMMY`) |
| Probe TEXT longer than 68 characters | SQL Msg 8152 string or binary data would be truncated |
| Long absolute paths in the ADDTO argument | Same 68-char trap; prefer short `../../tmp/…` |

`FORMAT ADDTO` **appends**; delete or rename the file between UI runs so
chronology stays clear.

## How to instrument

1. Backup the live `FORMCLTRIGTEXT` / `FORMTRIGTEXT` rows for the TRIG ids you
   will touch.
2. Insert short probes **before and after** each suspect step:
   - column POST that fills related fields
   - `#INCLUDE …/BUF*` bodies
   - CHECK-FIELD / nested POSTs that re-enter warehouse or location logic
3. Form Prep the form (`EXECPREPLOCK.UPD='N'` and `LASTPREPDATE` advanced).
4. Reproduce once in the UI.
5. Read the ADDTO file in order. The first tag where the value becomes wrong
   is the wipe (or fill) boundary.
6. Strip every probe after diagnosis; Form Prep again; retest UI.

Tag names should be 2–4 characters (`A1`, `Wa`, `WNb`) so the TEXTLINE fits.

## Related wipe pattern (warehouse transfer / LOCNAME)

Observed on warehouse-transfer forms when From Bin (`LOCNAME`) becomes `0`:

1. Nested `WARHSNAME` → `#INCLUDE WAREHOUSES/BUF1` can default the bin to `0`.
2. `LOCNAME` can already be `0` at the **start** of `WARHS` POST (before
   `TRANSTRIG/BUF21`) — prove this with ADDTO tags around ACTNAME POST end
   and WARHS POST start.
3. After BUF includes, **re-assert** the stock bin with a join on
   `ACT` / `WARHSBAL` / `WAREHOUSES` / `SERIAL` where `BALANCE > 0` and
   `LOCNAME <> '0'`. Prefer the stock bin column from the balance row, not a
   bare `WAREHOUSES.LOCNAME` that binds the **form WAREHOUSES buffer**.
4. Do **not** use `WHERE WARHS = :$.WARHS` alone after BUF1 when the form
   buffer’s warehouse points at a master row whose bin is `0`.

## Cross-links

- Line length / style: `PROCEDURE_STYLE.md` / **priority-procedure-style**
- Form Prep after SQL trigger edits: **priority-form-prep-after-sql-change**
- Form Prep gate: **priority-formprep**
- Official SDK notes on `FORMAT` / `ADDTO`: Medatech network
  `forms-code` / SOP “SDK for Priority” (lazy-load via priority-skill-dag)
