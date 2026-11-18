# Requirements Analysis Questions: expense balance columns

Request: add the columns (Accrual)/Prepaid, Cumulative Balance, ST Accrued Liability, LT Accrued Liability, ST Prepaid Asset and LT Prepaid Asset to the expense schedule, and show negative values as (1000) instead of with a minus sign.

Today each expense row is one period (a calendar month for Overtime, one date for All at once) with an expense amount. Payments and expenses are calculated separately and never compared.

## Question 1
How should the (Accrual)/Prepaid amount of a row be worked out?

A. Payments due in the row's period minus the row's expense. Positive means prepaid (paid ahead of the expense); negative means accrued (expense ahead of payment) and shows in brackets
B. Expense minus payments due in the row's period (the opposite sign: positive means accrued)
X. Other (please specify)

[Answer]: A

## Question 2
What is the Cumulative Balance?

A. The running total of (Accrual)/Prepaid from the first row to this row, so it ends at 0 on the last row
B. Total payments due up to the end of the row's period minus total expense up to the end of that period (the same number as A, defined from the totals)
X. Other (please specify)

[Answer]: A

## Question 3
Payments can fall in months with no expense (before the expense starts, after it ends, or between dates for All at once). How should those payments show?

A. Add a row for each such month with expense 0, so every month that has a payment or an expense appears, in date order
B. Keep only the expense rows; fold payments before the first row into the first row and payments after the last row into the last row
X. Other (please specify)

[Answer]: A

## Question 4
How should a row's Cumulative Balance be split between short term (ST) and long term (LT)?

A. ST is the part of the balance that reverses within 12 months after the row's period end, according to the rest of the schedule; LT is the remainder
B. ST is the whole balance when the contract ends within 12 months of the row's period end; otherwise all of it is LT
C. All of the balance is ST (no LT split)
X. Other (please specify)

[Answer]: A

## Question 5
Which columns does each sign of the balance fill?

A. A negative balance fills ST and LT Accrued Liability, and the Prepaid Asset columns show 0; a positive balance fills ST and LT Prepaid Asset, and the Accrued Liability columns show 0. Liability and asset amounts are shown as positive numbers
B. The same split, but the liability amounts keep the negative sign and show in brackets
X. Other (please specify)

[Answer]: A

## Question 6
How should amounts look after the change?

A. Negative amounts in brackets with thousands separators and 2 decimals, for example (1,000.00); positive as 1,000.00. This applies to every amount and total in the payment schedule, the expense schedule and the preview popups
B. Negative amounts in brackets with no thousands separators, for example (1000.00); positive unchanged as 1000.00
X. Other (please specify)

[Answer]: A

## Question 7
What should the total row of the expense schedule show?

A. Totals for Expense and (Accrual)/Prepaid; the Cumulative Balance and the four ST and LT columns are left blank in the total row
B. Totals for Expense and (Accrual)/Prepaid, and the balance columns show the last row's values
X. Other (please specify)

[Answer]: X. Other (please specify): no need for total row (given in the reply as "Q3: no need for total row"; Q3 was already answered, so it is read as the answer to Q7)
