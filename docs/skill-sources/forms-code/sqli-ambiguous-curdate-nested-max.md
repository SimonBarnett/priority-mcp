# SQLI: Ambiguous CURDATE with nested MAX subquery

## Symptom

Priority SQLI / WINDBI:

```
SQL Server Error (80040e14)
Ambiguous column name 'CURDATE'. (Error 209)
```

when a SELECT compares `D.CURDATE` to a nested `SELECT MAX(D2.CURDATE) FROM …`.

Even with aliases on both sides, the Priority SQL translator can expand the
subquery into a shape SQL Server rejects.

## Fix

Two steps into a `:VAR`, then filter:

```
:ROOCURD = 0;
SELECT MAX(D.CURDATE) INTO :ROOCURD
FROM DETAILEDCOST D
WHERE D.PART = :PART
AND D.SERIAL = -3
AND D.FULL = 'Y'
AND D.FLAG = 'C'
;
SELECT D.EMP1 INTO :ROOLABOUR
FROM DETAILEDCOST D
WHERE D.PART = :PART
AND D.SERIAL = -3
AND D.FULL = 'Y'
AND D.FLAG = 'C'
AND D.CURDATE = :ROOCURD
;
```

Same pattern for any Priority date column that appears in both outer and
nested MAX when prep or run hits Error 209.

## Seen

Customer Test (Parvalux SO260342) labour pull from `DETAILEDCOST` inside a
cost-split include. Generic lesson only; no customer hosts or secrets.
