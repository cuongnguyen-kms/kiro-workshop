using ContractManagement.Scheduling;

namespace ContractManagement.Api.Domain;

/// <summary>A saved contract: the aggregate root that owns its streams and both schedules.</summary>
public sealed class Contract
{
    /// <summary>Identifier, generated when the contract is built.</summary>
    public Guid ContractId { get; set; }
    /// <summary>Trimmed name, 1 to 200 characters. Duplicates are allowed (BR1.3).</summary>
    public string Name { get; set; } = "";
    /// <summary>First day of the contract.</summary>
    public DateOnly StartDate { get; set; }
    /// <summary>Last day of the contract.</summary>
    public DateOnly EndDate { get; set; }
    /// <summary>How the expense is booked.</summary>
    public ExpenseMethod ExpenseMethod { get; set; }
    /// <summary>Overtime range start. Set only for Overtime.</summary>
    public DateOnly? ExpenseStartDate { get; set; }
    /// <summary>Overtime range end. Set only for Overtime.</summary>
    public DateOnly? ExpenseEndDate { get; set; }
    /// <summary>Single expense date. Set only for AllAtOnce.</summary>
    public DateOnly? ExpenseDate { get; set; }
    /// <summary>Time of save, for audit only.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>The payment streams, in request order.</summary>
    public List<PaymentStream> PaymentStreams { get; set; } = [];
    /// <summary>The generated payment rows.</summary>
    public List<PaymentScheduleEntry> PaymentEntries { get; set; } = [];
    /// <summary>The generated expense rows.</summary>
    public List<ExpenseScheduleEntry> ExpenseEntries { get; set; } = [];
}

/// <summary>One way the contract pays out.</summary>
public sealed class PaymentStream
{
    /// <summary>Identifier.</summary>
    public Guid StreamId { get; set; }
    /// <summary>The owning contract.</summary>
    public Guid ContractId { get; set; }
    /// <summary>Order of the stream in the request.</summary>
    public int Position { get; set; }
    /// <summary>Amount of each payment, greater than zero, at most 2 decimals.</summary>
    public decimal Amount { get; set; }
    /// <summary>How often the stream pays.</summary>
    public Frequency Frequency { get; set; }
    /// <summary>First date.</summary>
    public DateOnly StartDate { get; set; }
    /// <summary>Last date. Equals StartDate for OneTime.</summary>
    public DateOnly EndDate { get; set; }
}

/// <summary>One generated payment.</summary>
public sealed class PaymentScheduleEntry
{
    /// <summary>Identifier.</summary>
    public Guid PaymentEntryId { get; set; }
    /// <summary>The owning contract.</summary>
    public Guid ContractId { get; set; }
    /// <summary>The stream that produced the row.</summary>
    public Guid StreamId { get; set; }
    /// <summary>Date the payment is due.</summary>
    public DateOnly DueDate { get; set; }
    /// <summary>Payment amount.</summary>
    public decimal Amount { get; set; }
}

/// <summary>One generated expense row.</summary>
public sealed class ExpenseScheduleEntry
{
    /// <summary>Identifier.</summary>
    public Guid ExpenseEntryId { get; set; }
    /// <summary>The owning contract.</summary>
    public Guid ContractId { get; set; }
    /// <summary>First day the row covers.</summary>
    public DateOnly PeriodStart { get; set; }
    /// <summary>Last day the row covers.</summary>
    public DateOnly PeriodEnd { get; set; }
    /// <summary>Expense amount.</summary>
    public decimal Amount { get; set; }
}
