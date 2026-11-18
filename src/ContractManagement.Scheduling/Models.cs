namespace ContractManagement.Scheduling;

/// <summary>How often a payment stream pays.</summary>
public enum Frequency
{
    /// <summary>Every 7 days.</summary>
    Weekly,
    /// <summary>Every month, on the start day (cut back in short months).</summary>
    Monthly,
    /// <summary>Every 3 months.</summary>
    Quarterly,
    /// <summary>Every 12 months.</summary>
    Annually,
    /// <summary>One payment on the start date.</summary>
    OneTime
}

/// <summary>How the total payment amount is booked as an expense.</summary>
public enum ExpenseMethod
{
    /// <summary>Spread per day over a date range, one row per calendar month.</summary>
    Overtime,
    /// <summary>The whole amount on one date.</summary>
    AllAtOnce,
    /// <summary>No expense schedule.</summary>
    None
}

/// <summary>One payment stream of a contract.</summary>
/// <param name="StreamId">Identifier copied onto every payment of this stream.</param>
/// <param name="Amount">Amount of each payment; must be above zero.</param>
/// <param name="Frequency">How often it pays.</param>
/// <param name="StartDate">Date of the first payment.</param>
/// <param name="EndDate">Last date a payment may fall on; equals StartDate for OneTime.</param>
public sealed record StreamInput(
    Guid StreamId, decimal Amount, Frequency Frequency, DateOnly StartDate, DateOnly EndDate);

/// <summary>How the expense is booked.</summary>
/// <param name="Method">The expense method.</param>
/// <param name="StartDate">Overtime range start (Overtime only).</param>
/// <param name="EndDate">Overtime range end, included (Overtime only).</param>
/// <param name="Date">Expense date (AllAtOnce only).</param>
public sealed record ExpenseInput(
    ExpenseMethod Method, DateOnly? StartDate, DateOnly? EndDate, DateOnly? Date);

/// <summary>Input of <see cref="IScheduleGenerator.Generate"/>.</summary>
/// <param name="Streams">The payment streams, in order.</param>
/// <param name="Expense">The expense settings.</param>
public sealed record ScheduleRequest(IReadOnlyList<StreamInput> Streams, ExpenseInput Expense);

/// <summary>One scheduled payment.</summary>
/// <param name="StreamId">The stream it belongs to.</param>
/// <param name="DueDate">Date the payment is due.</param>
/// <param name="Amount">Payment amount.</param>
public sealed record PaymentEntry(Guid StreamId, DateOnly DueDate, decimal Amount);

/// <summary>One expense row.</summary>
/// <param name="PeriodStart">First day the row covers.</param>
/// <param name="PeriodEnd">Last day the row covers.</param>
/// <param name="Amount">Expense amount, 2 decimal places.</param>
public sealed record ExpenseEntry(DateOnly PeriodStart, DateOnly PeriodEnd, decimal Amount);

/// <summary>Both schedules and their totals.</summary>
/// <param name="Payments">Payments ordered by due date, then stream position.</param>
/// <param name="PaymentTotal">Sum of all payment amounts.</param>
/// <param name="Expenses">Expense rows ordered by period start.</param>
/// <param name="ExpenseTotal">Sum of all expense amounts.</param>
/// <param name="Balance">Monthly expense balance rows; empty when there are no expense rows.</param>
public sealed record ScheduleResult(
    IReadOnlyList<PaymentEntry> Payments, decimal PaymentTotal,
    IReadOnlyList<ExpenseEntry> Expenses, decimal ExpenseTotal,
    IReadOnlyList<BalanceRow> Balance);

/// <summary>One calendar month of the expense balance schedule (calculated, never stored).</summary>
/// <param name="PeriodStart">First day of the month, or the Overtime row's period start.</param>
/// <param name="PeriodEnd">Last day of the month, or the Overtime row's period end.</param>
/// <param name="Expense">Expense of the month; 0 for a payment-only month.</param>
/// <param name="AccrualPrepaid">Payments due in the month minus expense; positive is prepaid, negative is accrued.</param>
/// <param name="CumulativeBalance">Running total of AccrualPrepaid; 0 on the last row.</param>
/// <param name="StAccruedLiability">Short-term accrued liability (reverses within 12 months); never negative.</param>
/// <param name="LtAccruedLiability">Long-term accrued liability; never negative.</param>
/// <param name="StPrepaidAsset">Short-term prepaid asset; never negative.</param>
/// <param name="LtPrepaidAsset">Long-term prepaid asset; never negative.</param>
public sealed record BalanceRow(
    DateOnly PeriodStart, DateOnly PeriodEnd, decimal Expense, decimal AccrualPrepaid,
    decimal CumulativeBalance, decimal StAccruedLiability, decimal LtAccruedLiability,
    decimal StPrepaidAsset, decimal LtPrepaidAsset);

/// <summary>Contract C2: builds the monthly balance rows from payment and expense rows.</summary>
public interface IBalanceScheduleBuilder
{
    /// <summary>Pure and deterministic. Returns no rows when <paramref name="expenses"/> is empty.</summary>
    /// <exception cref="ArgumentNullException">A list is null.</exception>
    /// <exception cref="ArgumentException">Expense rows exist and the payment and expense totals differ.</exception>
    IReadOnlyList<BalanceRow> Build(IReadOnlyList<PaymentEntry> payments, IReadOnlyList<ExpenseEntry> expenses);
}

/// <summary>Contract C2: the schedule engine.</summary>
public interface IScheduleGenerator
{
    /// <summary>
    /// Pure and deterministic. Payments are ordered by DueDate, then by stream order.
    /// Expenses are ordered by PeriodStart. ExpenseTotal equals PaymentTotal unless Method is None.
    /// </summary>
    /// <exception cref="ArgumentException">The input breaks a precondition.</exception>
    ScheduleResult Generate(ScheduleRequest request);
}
