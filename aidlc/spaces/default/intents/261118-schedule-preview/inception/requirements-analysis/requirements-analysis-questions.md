# Requirements Analysis Questions: schedule preview

Request: view the payment schedule and the expense schedule while creating a contract, through two buttons that open a popup each, once enough information is on the page.

## Question 1
When should the two preview buttons be usable?

A. Only when the whole form passes the same checks as Save; until then they are disabled
B. Each button on its own: "Payment schedule" when the contract dates and every stream are valid; "Expense schedule" when, in addition, the expense method and its dates are valid
C. Always clickable; clicking with missing or invalid information shows the form's error messages instead of the popup
X. Other (please specify)

[Answer]: B

## Question 2
Where should the preview be calculated?

A. On the server, through a new preview call that checks and calculates with the same rules as Save but stores nothing, so the preview always matches what will be saved
B. In the browser, with a copy of the schedule rules (works offline, but the rules then exist twice and could disagree)
X. Other (please specify)

[Answer]: A

## Question 3
What should the expense schedule button do when the expense method is None?

A. Stay disabled, with a hint that this contract has no expense schedule
B. Open the popup with the message "This contract has no expense schedule."
C. Be hidden
X. Other (please specify)

[Answer]: A

## Question 4
What should each popup show?

A. The same table as the saved contract's schedule tab (date or period, stream, amount) with the total
B. The table and total, plus a short summary above it (number of rows, first and last date)
X. Other (please specify)

[Answer]: A
