namespace ContractManagement.Api.Contracts;

/// <summary>Which schedule a preview is for. A payment preview ignores the expense fields (FR2.3).</summary>
public enum PreviewKind
{
    /// <summary>Payment schedule only; the expense fields are not checked.</summary>
    Payment,

    /// <summary>Both schedules; the expense fields are checked.</summary>
    Expense,
}

/// <summary>One previewed payment row. <c>StreamNumber</c> is the 1-based position of the stream in the request (FR3.3).</summary>
public sealed record PreviewPaymentEntryResponse(int StreamNumber, DateOnly DueDate, decimal Amount);

/// <summary>The previewed payment schedule and its total.</summary>
public sealed record PreviewPaymentScheduleResponse(IReadOnlyList<PreviewPaymentEntryResponse> Entries, decimal Total);

/// <summary>Response of <c>POST /api/v1/contracts/preview</c>: both schedules, nothing stored.</summary>
public sealed record SchedulePreviewResponse(PreviewPaymentScheduleResponse PaymentSchedule, ExpenseScheduleResponse ExpenseSchedule);
