namespace ContractManagement.Scheduling;

/// <summary>Stateless implementation of <see cref="IScheduleGenerator"/>.</summary>
public sealed class ScheduleGenerator : IScheduleGenerator
{
    /// <inheritdoc />
    public ScheduleResult Generate(ScheduleRequest request)
    {
        InputChecker.Check(request);
        var (payments, paymentTotal) = PaymentScheduleBuilder.Build(request.Streams);
        var (expenses, expenseTotal) = ExpenseScheduleBuilder.Build(request.Expense, paymentTotal);
        return new ScheduleResult(payments.AsReadOnly(), paymentTotal, expenses.AsReadOnly(), expenseTotal);
    }
}
