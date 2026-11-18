namespace ContractManagement.Scheduling;

/// <summary>Stateless implementation of <see cref="IScheduleGenerator"/>.</summary>
public sealed class ScheduleGenerator : IScheduleGenerator
{
    private static readonly BalanceScheduleBuilder BalanceBuilder = new();

    /// <inheritdoc />
    public ScheduleResult Generate(ScheduleRequest request)
    {
        InputChecker.Check(request);
        var (payments, paymentTotal) = PaymentScheduleBuilder.Build(request.Streams);
        var (expenses, expenseTotal) = ExpenseScheduleBuilder.Build(request.Expense, paymentTotal);
        var paymentRows = payments.AsReadOnly();
        var expenseRows = expenses.AsReadOnly();
        return new ScheduleResult(
            paymentRows, paymentTotal, expenseRows, expenseTotal, BalanceBuilder.Build(paymentRows, expenseRows));
    }
}
