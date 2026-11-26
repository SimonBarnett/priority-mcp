# Form trigger `:VAR` types (Form Prep)

## Error

```
Variable with two different types : SORT
```

(or another `:VAR` name)

## Meaning

During `FORMPREPDRCT2` / Form Preparation, Priority assigned two different
scalar types to the same `:VAR` in one compile unit (trigger + includes +
related forms).

## Fix pattern

1. Find all `:VAR` assignments / `INTO :VAR` in the failing form and its
   `#INCLUDE` / sibling triggers.
2. Keep **one type per name**. Prefer renaming over overloading:
   - CHAR / STRIND → `:DWFIXSORT`, `:CURFIX`, …
   - INT / MAX(col) → `:SORT`, `:KLINE`, …
3. Re-prep the form (`Prepare-NamedForm.ps1 -Name … -ForceUnprepared`).
4. Confirm the SORT/type warning is **gone** from SDK `errors[]`.
5. SQL gate still required: `UPD=N` and `LASTPREPDATE` advanced.

## CE example (2026-09-28)

`ZCLA_ELEDITSPLIT` TRIG 22: `:SORT = ''` then `INTO :SORT` from
`STRIND(ITOA(FIX),…)`, while `ZCLA_FIXACT` uses numeric `:SORT`.
Renamed CHAR use to `:DWFIXSORT`.

## INT versus REAL (quantity)

`:VAR = 0` is an integer. `SUM` of a dictionary INT column (stock `QUANT` is INT) stays an integer. Writing that variable into a REAL column types it as REAL as well. Form Prep then reports:

```
Variable with two different types : QTY
```

Keep one type. For a REAL target:

```
:QTY = 0.0;
SELECT 0.0 + SUM(QUANT) INTO :QTY FROM <movement table>
```

Do not assign an INT `:VAR` into the REAL `:VAR`. A second `:QTY = 0` later in the same trigger reintroduces the integer.

A shifted quantity is the other way around. `:QTY = 0` and `SUM` of `QUANT` are INT. `SELECT :QTY - :QEX INTO :QTY FROM DUMMY` is REAL. That is the same error. Use `:QTY = :QTY - :QEX`. See `SHIFTED_INTEGERS.md`.

## Standard trigger ids

`system.TRIGGERS` is the name map. On a current on-prem dictionary:

| TRIG | TRIGNAME |
|------|----------|
| -1 | CHOOSE-FIELD |
| 7 | PRE-FORM |

Do not store a pre-form body on TRIG -1. Resolve the id:

```sql
SELECT TRIG FROM TRIGGERS WHERE TRIGNAME = N'PRE-FORM';
```

## Related

- `PROCEDURE_STYLE.md` / catalog `priority-procedure-style`
- `priority-formprep` (WCF prep warnings)
- `priority-uat-wcf` (`startSubForm` when direct `formStart` says unprepared)
