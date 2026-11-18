# Mockups: expense balance columns

Built directly from the user stories and requirements: the classic plan has no rough mockups. Only the expense table and the amount format change. The rest of S2 (New contract form) and S3 (Contract detail) stay as they are.

## M1. Expense schedule tab on the contract detail screen (S3) - US1.1, US2.1, US2.2, US2.3, US3.1

```
+----------------------------------------------------------------------------------------------+
| [ Payment schedule ] [ Expense schedule* ]                                                    |
|                                                                                               |
| +-----------+------------+-------------------+------------------+-----------+-----------+--> |
| | Period    | Expense    | (Accrual)/Prepaid | Cumulative       | ST        | LT        |    |
| | (sticky)  |            |                   | Balance          | Accrued   | Accrued   | .. |
| |           |            |                   |                  | Liability | Liability |    |
| +-----------+------------+-------------------+------------------+-----------+-----------+--> |
| | Jan 2027  |   1,000.00 |         11,000.00 |        11,000.00 |      0.00 |      0.00 |    |
| | Feb 2027  |   1,000.00 |        (1,000.00) |        10,000.00 |      0.00 |      0.00 |    |
| | ...       |            |                   |                  |           |           |    |
| | Dec 2027  |   1,000.00 |        (1,000.00) |             0.00 |      0.00 |      0.00 |    |
| +-----------+------------+-------------------+------------------+-----------+-----------+--> |
|   <================= horizontal scroll bar (Period column stays in view) ==================>  |
+----------------------------------------------------------------------------------------------+
```

Columns, in order: Period, Expense, (Accrual)/Prepaid, Cumulative Balance, ST Accrued Liability, LT Accrued Liability, ST Prepaid Asset, LT Prepaid Asset (the last two are off to the right in the sketch).

- Headers stay on one line each; the table is as wide as its content and scrolls sideways inside its card when it does not fit (Q1).
- The Period column stays fixed at the left while the rest scrolls (Q1).
- Period shows month and year, for example "Jan 2027", for every row: expense months, payment-only months and the All at once month (Q4).
- Every amount column is right-aligned with tabular figures so decimals line up.
- No total row (US2.3).
- Negative amounts show in brackets in the normal text colour (Q3).

The full January to December example (12,000.00 paid in January, 1,000.00 expense a month):

| Period | Expense | (Accrual)/Prepaid | Cumulative Balance | ST Accrued Liability | LT Accrued Liability | ST Prepaid Asset | LT Prepaid Asset |
|---|---|---|---|---|---|---|---|
| Jan 2027 | 1,000.00 | 11,000.00 | 11,000.00 | 0.00 | 0.00 | 11,000.00 | 0.00 |
| Feb 2027 | 1,000.00 | (1,000.00) | 10,000.00 | 0.00 | 0.00 | 10,000.00 | 0.00 |
| ... | | | | | | | |
| Dec 2027 | 1,000.00 | (1,000.00) | 0.00 | 0.00 | 0.00 | 0.00 | 0.00 |

## M2. Expense schedule preview popup on the New contract form (S2) - US2.1 AC2.1.4, US2.3 AC2.3.2

```
+------------------------------- page ----------------------------------+
|        +------------- popup: up to 80% of the screen width --------+   |
|        | Expense schedule                                     [x]  |   |
|        | Preview of the expense schedule for the details entered.  |   |
|        |                                                           |   |
|        | | Period   | Expense | (Accrual)/Prepaid | Cumulative ..-> |   |
|        | | Jan 2027 |    0.00 |          3,000.00 |    3,000.00 ..-> |   |
|        | | Mar 2027 | 1,000.00|        (1,000.00) |    2,000.00 ..-> |   |
|        |   <=========== horizontal scroll inside the popup ======>  |   |
|        |                                                [ Close ]  |   |
|        +-----------------------------------------------------------+   |
+-----------------------------------------------------------------------+
```

- The Expense schedule popup is up to 80% of the screen width (Q2). The table scrolls sideways inside it when it does not fit, with the Period column kept in view.
- The popup keeps its height limit and scrolls down for long schedules, with the column headers staying at the top.
- Same columns, rows and formats as M1 (FR2.6).
- The Payment schedule popup keeps its current width.

## M3. Payment schedule (tab and popup) - US3.1 AC3.1.4

Unchanged columns (Due date, Stream, Amount) and total row. Only the number format changes: 1,000.00 and (1,000.00).

## M4. Other amounts on S2 and S3

Every amount shown in the app (stream amounts, stream summaries, totals) uses the new format, because all of them go through the one shared money formatter (US3.1 AC3.1.1 to AC3.1.3).

## States

| State | M1 tab | M2 popup |
|---|---|---|
| Loading | unchanged (detail screen skeleton) | unchanged (popup loading message) |
| Expense method None | unchanged: "This contract has no expense schedule." | button stays disabled with its hint (unchanged) |
| Error | unchanged | unchanged (retry message and button) |
| Long schedule (10 years) | table scrolls down inside the page; headers sticky | table scrolls inside the popup; headers sticky |
