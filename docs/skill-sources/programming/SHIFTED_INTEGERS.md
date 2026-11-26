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

`SELECT :QTY - :OTHER INTO :QTY FROM DUMMY` is a real expression. `:QTY = 0` and `SUM` of an INT quantity are integers. Form Prep then reports `Variable with two different types : QTY`. Subtract with an integer assignment:

```
:QTY = :QTY - :QEX;
```

`REALQUANT` is only for a real result (quantity times a real price). It is not the way to store the sum.
