# Shifted integers

Priority-generic. Source: Priority SDK, Decimal Precision, and `REALQUANT` / `INTQUANT`.

A quantity such as `TRANSORDER.QUANT` or `TQUANT` is an **INT** with decimal precision **3** (the `DECIMAL` system constant, usually 3). The SQL column is `bigint`. The stored value is the business quantity times `10^precision`.

| Stored | Displayed |
|--------|-----------|
| 1000 | 1.000 |
| 2563000 | 2563.000 |

`REALQUANT(m)` turns the stored integer into the displayed real. `INTQUANT(m)` does the reverse. Example from the SDK: `:TQUANT = 1000` then `REALQUANT` returns `1.000` when `DECIMAL` is 3.

## Trigger rule

Sum a shifted quantity into a column defined the same way: `INT`, width **17**, precision **3**, SQL `bigint`. Copy the source title when it is a factory quantity: `Qty (Factory Units)`. Set the form column `DEC` to 3.

```
:QTY = 0;
SELECT SUM(QUANT) INTO :QTY FROM <movement table> WHERE ...
```

Do not add `0.0`. Do not divide by 1000. Both turn the value into a real. The extra zeros are already in the stored integer, so folding them into a REAL or a narrow integer overflows. The form displays the business quantity because the column precision is 3.

Form Prep types `SUM` of that INT as **real**. Every integer write of the same variable is then reported on its own. Three writes (`:QTY = 0`, a second `:QTY = 0`, and `:QTY = :QTY - :QEX`) produce the message three times:

```
Variable with two different types : QTY
```

`SELECT :QTY - :QEX INTO :QTY FROM DUMMY` is also real. An assignment of that subtraction does not clear the error once `SUM` has typed the variable real.

Bring the value back to a shifted integer with the SDK pair `REALQUANT` / `INTQUANT`. `INTQUANT` is the only write, apart from `:QTY = 0`:

```
:QTY = 0;
SELECT INTQUANT(REALQUANT(SUM(QUANT))) INTO :QTY
FROM <movement table> WHERE ...
:NET = 0;
SELECT INTQUANT(REALQUANT(:QTY) - REALQUANT(:QEX))
INTO :NET FROM DUMMY;
```

Insert `:NET`. Do not put `:QTY` in the same `SELECT` list as a CHAR literal; that is a second type. `REALQUANT` alone is for a real result (quantity times a price), not for the stored column.
