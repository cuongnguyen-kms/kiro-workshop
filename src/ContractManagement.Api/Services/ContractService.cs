using ContractManagement.Api.Contracts;
using ContractManagement.Api.Domain;
using ContractManagement.Api.Persistence;
using ContractManagement.Api.Validation;
using ContractManagement.Scheduling;
using SchedulingExpenseInput = ContractManagement.Scheduling.ExpenseInput;

namespace ContractManagement.Api.Services;

/// <summary>Outcome of a create: the saved contract, or the validation errors keyed by field path.</summary>
public sealed record CreateContractResult(Contract? Contract, IReadOnlyDictionary<string, string[]>? Errors)
{
    /// <summary>True when the contract was saved.</summary>
    public bool Succeeded => Contract is not null;
}

/// <summary>Outcome of a preview: both schedules, or the validation errors keyed by field path.</summary>
public sealed record PreviewResult(SchedulePreviewResponse? Preview, IReadOnlyDictionary<string, string[]>? Errors)
{
    /// <summary>True when the schedules were generated.</summary>
    public bool Succeeded => Preview is not null;
}

/// <summary>Creates, reads and lists contracts. Validates, generates both schedules through <see cref="IScheduleGenerator"/> and saves everything in one operation.</summary>
public sealed class ContractService(
    IScheduleGenerator generator,
    IContractRepository repository,
    TimeProvider timeProvider,
    ILogger<ContractService> logger)
{
    /// <summary>Validates, generates and saves a contract (BR1.1 to BR5.4). Throws when the engine or the repository fails; nothing is saved then.</summary>
    public async Task<CreateContractResult> CreateAsync(CreateContractRequest? request, CancellationToken cancellationToken)
    {
        var errors = ContractValidator.Validate(request);
        if (!errors.IsEmpty)
        {
            logger.LogInformation("Validation failed for fields {Fields}", string.Join(", ", errors.ToDictionary().Keys));
            return new CreateContractResult(null, errors.ToDictionary());
        }

        var contractId = Guid.NewGuid();
        var streams = BuildStreams(request!, contractId);
        var expense = BuildExpense(request!);

        // BR5.4: refuse an oversized contract before the engine runs.
        var scheduleRequest = new ScheduleRequest(
            streams.Select(s => new StreamInput(s.StreamId, s.Amount, s.Frequency, s.StartDate, s.EndDate)).ToList(),
            new SchedulingExpenseInput(expense.Method, expense.StartDate, expense.EndDate, expense.Date));
        if (SizeEstimator.Estimate(scheduleRequest) > SizeEstimator.MaxRows)
        {
            var tooLarge = new ValidationErrors();
            tooLarge.Add("paymentStreams", SizeEstimator.TooLarge);
            logger.LogInformation("Contract rejected: estimated more than {Max} schedule rows", SizeEstimator.MaxRows);
            return new CreateContractResult(null, tooLarge.ToDictionary());
        }

        var result = generator.Generate(scheduleRequest);
        CheckTotals(result, expense.Method);

        var contract = new Contract
        {
            ContractId = contractId,
            Name = request!.Name!.Trim(),
            StartDate = request.StartDate!.Value,
            EndDate = request.EndDate!.Value,
            ExpenseMethod = expense.Method,
            ExpenseStartDate = expense.StartDate,
            ExpenseEndDate = expense.EndDate,
            ExpenseDate = expense.Date,
            CreatedAt = timeProvider.GetUtcNow(),
            PaymentStreams = streams,
            PaymentEntries = result.Payments.Select(p => new PaymentScheduleEntry
            {
                PaymentEntryId = Guid.NewGuid(), ContractId = contractId, StreamId = p.StreamId, DueDate = p.DueDate, Amount = p.Amount,
            }).ToList(),
            ExpenseEntries = result.Expenses.Select(e => new ExpenseScheduleEntry
            {
                ExpenseEntryId = Guid.NewGuid(), ContractId = contractId, PeriodStart = e.PeriodStart, PeriodEnd = e.PeriodEnd, Amount = e.Amount,
            }).ToList(),
        };

        await repository.SaveAsync(contract, cancellationToken);
        logger.LogInformation("Saved contract {ContractId} with {Streams} streams, {Payments} payment rows and {Expenses} expense rows",
            contractId, contract.PaymentStreams.Count, contract.PaymentEntries.Count, contract.ExpenseEntries.Count);
        return new CreateContractResult(contract, null);
    }

    /// <summary>
    /// Validates and generates the schedules exactly as create would, without saving anything (FR2.1, FR2.2, NFR2).
    /// The name is not needed (FR2.4); a payment preview ignores the expense fields (FR2.3).
    /// </summary>
    public PreviewResult Preview(CreateContractRequest? request, PreviewKind kind)
    {
        request ??= new CreateContractRequest(null, null, null, null, null);
        var effective = request with
        {
            Name = "preview",
            Expense = kind == PreviewKind.Payment ? new ExpenseRequest("None", null, null, null) : request.Expense,
        };

        var errors = ContractValidator.Validate(effective);
        if (!errors.IsEmpty)
        {
            return new PreviewResult(null, errors.ToDictionary());
        }

        var streams = BuildStreams(effective, Guid.NewGuid());
        var expense = BuildExpense(effective);
        var scheduleRequest = new ScheduleRequest(
            streams.Select(s => new StreamInput(s.StreamId, s.Amount, s.Frequency, s.StartDate, s.EndDate)).ToList(),
            new SchedulingExpenseInput(expense.Method, expense.StartDate, expense.EndDate, expense.Date));
        if (SizeEstimator.Estimate(scheduleRequest) > SizeEstimator.MaxRows)
        {
            var tooLarge = new ValidationErrors();
            tooLarge.Add("paymentStreams", SizeEstimator.TooLarge);
            return new PreviewResult(null, tooLarge.ToDictionary());
        }

        var result = generator.Generate(scheduleRequest);
        CheckTotals(result, expense.Method);

        var numbers = streams.ToDictionary(s => s.StreamId, s => s.Position + 1);
        var payments = result.Payments.Select(p => new PreviewPaymentEntryResponse(numbers[p.StreamId], p.DueDate, p.Amount)).ToList();
        var expenses = result.Expenses.Select(e => new ExpenseEntryResponse(e.PeriodStart, e.PeriodEnd, e.Amount)).ToList();
        return new PreviewResult(
            new SchedulePreviewResponse(
                new PreviewPaymentScheduleResponse(payments, payments.Sum(p => p.Amount)),
                new ExpenseScheduleResponse(expenses, expenses.Sum(e => e.Amount))),
            null);
    }

    /// <summary>Reads one contract, or null when none has the id.</summary>
    public Task<Contract?> GetAsync(Guid contractId, CancellationToken cancellationToken) =>
        repository.GetAsync(contractId, cancellationToken);

    /// <summary>Reads one page of contracts.</summary>
    public Task<ContractListResult> ListAsync(int page, int pageSize, CancellationToken cancellationToken) =>
        repository.ListAsync(page, pageSize, cancellationToken);

    private static List<PaymentStream> BuildStreams(CreateContractRequest request, Guid contractId) =>
        request.PaymentStreams!.Select((s, i) =>
        {
            var frequency = ContractValidator.ParseFrequency(s!.Frequency)!.Value;
            var start = s.StartDate!.Value;
            return new PaymentStream
            {
                StreamId = Guid.NewGuid(),
                ContractId = contractId,
                Position = i,
                Amount = s.Amount!.Value,
                Frequency = frequency,
                StartDate = start,
                // BR2.5: a One-time stream stores its start date as its end date.
                EndDate = frequency == Frequency.OneTime ? start : s.EndDate!.Value,
            };
        }).ToList();

    // BR3.5: dates that do not belong to the method are not stored.
    private static (ExpenseMethod Method, DateOnly? StartDate, DateOnly? EndDate, DateOnly? Date) BuildExpense(CreateContractRequest request)
    {
        var method = ContractValidator.ParseMethod(request.Expense!.Method)!.Value;
        return method switch
        {
            ExpenseMethod.Overtime => (method, request.Expense.StartDate, request.Expense.EndDate, null),
            ExpenseMethod.AllAtOnce => (method, null, null, request.Expense.Date),
            _ => (method, null, null, null),
        };
    }

    // BR4.2: the engine's totals must agree with its rows, or nothing is saved.
    private static void CheckTotals(ScheduleResult result, ExpenseMethod method)
    {
        var paymentSum = result.Payments.Sum(p => p.Amount);
        var expenseSum = result.Expenses.Sum(e => e.Amount);
        var expectedExpense = method == ExpenseMethod.None ? 0m : paymentSum;
        if (paymentSum != result.PaymentTotal || expenseSum != result.ExpenseTotal || expenseSum != expectedExpense)
        {
            throw new InvalidOperationException("The schedule engine returned totals that do not match its rows.");
        }
    }
}
