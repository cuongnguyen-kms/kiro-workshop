using ContractManagement.Api.Domain;
using ContractManagement.Api.Persistence;
using ContractManagement.Scheduling;

namespace ContractManagement.Api.Contracts;

/// <summary>A saved contract with its streams and both schedules (schema ContractDetail in C1).</summary>
public sealed record ContractDetailResponse(
    Guid Id, string Name, DateOnly StartDate, DateOnly EndDate,
    IReadOnlyList<PaymentStreamResponse> PaymentStreams,
    ExpenseResponse Expense,
    PaymentScheduleResponse PaymentSchedule,
    ExpenseScheduleResponse ExpenseSchedule);

/// <summary>A payment stream as stored.</summary>
public sealed record PaymentStreamResponse(Guid Id, decimal Amount, Frequency Frequency, DateOnly StartDate, DateOnly EndDate);

/// <summary>The expense settings as stored; dates that do not belong to the method are null.</summary>
public sealed record ExpenseResponse(ExpenseMethod Method, DateOnly? StartDate, DateOnly? EndDate, DateOnly? Date);

/// <summary>One payment row.</summary>
public sealed record PaymentEntryResponse(Guid StreamId, DateOnly DueDate, decimal Amount);

/// <summary>One expense row.</summary>
public sealed record ExpenseEntryResponse(DateOnly PeriodStart, DateOnly PeriodEnd, decimal Amount);

/// <summary>The payment schedule and its total (the sum of the rows).</summary>
public sealed record PaymentScheduleResponse(IReadOnlyList<PaymentEntryResponse> Entries, decimal Total);

/// <summary>The expense schedule and its total (the sum of the rows).</summary>
public sealed record ExpenseScheduleResponse(IReadOnlyList<ExpenseEntryResponse> Entries, decimal Total);

/// <summary>One row of the contract list.</summary>
public sealed record ContractSummaryResponse(Guid Id, string Name, DateOnly StartDate, DateOnly EndDate);

/// <summary>One page of the contract list (schema ContractPage in C1).</summary>
public sealed record ContractPageResponse(IReadOnlyList<ContractSummaryResponse> Items, int Page, int PageSize, int TotalCount, int TotalPages);

/// <summary>Maps domain objects to the C1 response shapes. Totals are summed from the rows every time (BR4.2).</summary>
public static class ContractMapper
{
    /// <summary>Maps a contract to its detail response.</summary>
    public static ContractDetailResponse ToDetail(Contract c) => new(
        c.ContractId, c.Name, c.StartDate, c.EndDate,
        c.PaymentStreams.Select(s => new PaymentStreamResponse(s.StreamId, s.Amount, s.Frequency, s.StartDate, s.EndDate)).ToList(),
        new ExpenseResponse(c.ExpenseMethod, c.ExpenseStartDate, c.ExpenseEndDate, c.ExpenseDate),
        new PaymentScheduleResponse(
            c.PaymentEntries.Select(e => new PaymentEntryResponse(e.StreamId, e.DueDate, e.Amount)).ToList(),
            c.PaymentEntries.Sum(e => e.Amount)),
        new ExpenseScheduleResponse(
            c.ExpenseEntries.Select(e => new ExpenseEntryResponse(e.PeriodStart, e.PeriodEnd, e.Amount)).ToList(),
            c.ExpenseEntries.Sum(e => e.Amount)));

    /// <summary>Maps one repository page to the list response.</summary>
    public static ContractPageResponse ToPage(ContractListResult result, int page, int pageSize) => new(
        result.Items.Select(i => new ContractSummaryResponse(i.Id, i.Name, i.StartDate, i.EndDate)).ToList(),
        page, pageSize, result.TotalCount, (int)Math.Ceiling(result.TotalCount / (double)pageSize));
}
