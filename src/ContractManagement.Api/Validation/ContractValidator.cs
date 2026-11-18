using ContractManagement.Api.Contracts;
using ContractManagement.Scheduling;

namespace ContractManagement.Api.Validation;

/// <summary>Field path and message pairs collected by validation. Paths are camelCase, for example <c>paymentStreams[0].amount</c>.</summary>
public sealed class ValidationErrors
{
    private readonly Dictionary<string, List<string>> _errors = new(StringComparer.Ordinal);

    /// <summary>True when no message was added.</summary>
    public bool IsEmpty => _errors.Count == 0;

    /// <summary>Adds one message under a field path.</summary>
    public void Add(string path, string message)
    {
        if (!_errors.TryGetValue(path, out var list))
        {
            _errors[path] = list = [];
        }
        if (!list.Contains(message)) list.Add(message);
    }

    /// <summary>The messages by field path, in the shape of the C1 <c>errors</c> map.</summary>
    public Dictionary<string, string[]> ToDictionary() =>
        _errors.ToDictionary(kv => kv.Key, kv => kv.Value.ToArray(), StringComparer.Ordinal);
}

/// <summary>Checks a create request against BR1.1 to BR3.4 and collects every message (BR1.x, BR2.x, BR3.x).</summary>
public static class ContractValidator
{
    public const string NameRequired = "Contract name is required.";
    public const string NameTooLong = "Contract name must be 200 characters or fewer.";
    public const string ValidDate = "Enter a valid date.";
    public const string EndBeforeStart = "End date must not be before the start date.";
    public const string StreamRequired = "Add at least one payment stream.";
    public const string AmountPositive = "Amount must be greater than zero.";
    public const string AmountDecimals = "Amount can have at most 2 decimal places.";
    public const string StreamDatesWithin = "Stream dates must be within the contract dates.";
    public const string ChooseFrequency = "Choose a frequency.";
    public const string ChooseMethod = "Choose an expense method.";
    public const string ExpenseEndBeforeStart = "Expense end date must not be before the start date.";
    public const string ExpenseDatesWithin = "Expense dates must be within the contract dates.";
    public const string PageMin = "Page must be 1 or more.";
    public const string PageSizeAllowed = "Page size must be 10, 20, 50 or 100.";

    /// <summary>The page sizes the list accepts.</summary>
    public static readonly int[] AllowedPageSizes = [10, 20, 50, 100];

    /// <summary>Validates a create request. A null request is treated as an empty one.</summary>
    public static ValidationErrors Validate(CreateContractRequest? request)
    {
        var errors = new ValidationErrors();
        request ??= new CreateContractRequest(null, null, null, null, null);

        // BR1.1
        var name = request.Name?.Trim();
        if (string.IsNullOrEmpty(name)) errors.Add("name", NameRequired);
        else if (name.Length > 200) errors.Add("name", NameTooLong);

        // BR1.2
        if (request.StartDate is null) errors.Add("startDate", ValidDate);
        if (request.EndDate is null) errors.Add("endDate", ValidDate);
        var datesValid = request.StartDate is not null && request.EndDate is not null;
        if (datesValid && request.EndDate < request.StartDate)
        {
            errors.Add("endDate", EndBeforeStart);
            datesValid = false;
        }

        ValidateStreams(request, errors, datesValid);
        ValidateExpense(request, errors, datesValid);
        return errors;
    }

    /// <summary>Validates the list query (BR5.3).</summary>
    public static ValidationErrors ValidatePaging(int page, int pageSize)
    {
        var errors = new ValidationErrors();
        if (page < 1) errors.Add("page", PageMin);
        if (!AllowedPageSizes.Contains(pageSize)) errors.Add("pageSize", PageSizeAllowed);
        return errors;
    }

    /// <summary>Parses an exact-spelling frequency, or returns null (BR2.6).</summary>
    public static Frequency? ParseFrequency(string? value) =>
        value is not null && Enum.GetNames<Frequency>().Contains(value, StringComparer.Ordinal)
            ? Enum.Parse<Frequency>(value)
            : null;

    /// <summary>Parses an exact-spelling expense method, or returns null (BR3.1).</summary>
    public static ExpenseMethod? ParseMethod(string? value) =>
        value is not null && Enum.GetNames<ExpenseMethod>().Contains(value, StringComparer.Ordinal)
            ? Enum.Parse<ExpenseMethod>(value)
            : null;

    private static void ValidateStreams(CreateContractRequest request, ValidationErrors errors, bool datesValid)
    {
        // BR2.1
        if (request.PaymentStreams is null || request.PaymentStreams.Count == 0)
        {
            errors.Add("paymentStreams", StreamRequired);
            return;
        }

        for (var i = 0; i < request.PaymentStreams.Count; i++)
        {
            var stream = request.PaymentStreams[i] ?? new PaymentStreamRequest(null, null, null, null);
            var path = $"paymentStreams[{i}]";

            // BR2.2
            if (stream.Amount is null || stream.Amount <= 0) errors.Add($"{path}.amount", AmountPositive);
            else if (decimal.Round(stream.Amount.Value, 2) != stream.Amount.Value) errors.Add($"{path}.amount", AmountDecimals);

            // BR2.6
            var frequency = ParseFrequency(stream.Frequency);
            if (frequency is null) errors.Add($"{path}.frequency", ChooseFrequency);

            if (stream.StartDate is null) errors.Add($"{path}.startDate", ValidDate);

            // BR2.4 (a One-time stream ignores its end date, BR2.5)
            var effectiveEnd = frequency == Frequency.OneTime ? stream.StartDate : stream.EndDate;
            if (frequency is not null && frequency != Frequency.OneTime)
            {
                if (stream.EndDate is null) errors.Add($"{path}.endDate", ValidDate);
                else if (stream.StartDate is not null && stream.EndDate < stream.StartDate) errors.Add($"{path}.endDate", EndBeforeStart);
            }

            // BR2.3
            if (datesValid)
            {
                if (stream.StartDate is { } s && (s < request.StartDate || s > request.EndDate))
                    errors.Add($"{path}.startDate", StreamDatesWithin);
                if (frequency != Frequency.OneTime && effectiveEnd is { } e && (e < request.StartDate || e > request.EndDate))
                    errors.Add($"{path}.endDate", StreamDatesWithin);
            }
        }
    }

    private static void ValidateExpense(CreateContractRequest request, ValidationErrors errors, bool datesValid)
    {
        var expense = request.Expense;
        var method = ParseMethod(expense?.Method);
        if (method is null)
        {
            errors.Add("expense.method", ChooseMethod);
            return;
        }

        if (method == ExpenseMethod.Overtime)
        {
            if (expense!.StartDate is null) errors.Add("expense.startDate", ValidDate);
            if (expense.EndDate is null) errors.Add("expense.endDate", ValidDate);
            if (expense.StartDate is not null && expense.EndDate is not null && expense.EndDate < expense.StartDate)
                errors.Add("expense.endDate", ExpenseEndBeforeStart);
            if (datesValid)
            {
                if (expense.StartDate is { } s && (s < request.StartDate || s > request.EndDate)) errors.Add("expense.startDate", ExpenseDatesWithin);
                if (expense.EndDate is { } e && (e < request.StartDate || e > request.EndDate)) errors.Add("expense.endDate", ExpenseDatesWithin);
            }
        }
        else if (method == ExpenseMethod.AllAtOnce)
        {
            if (expense!.Date is null) errors.Add("expense.date", ValidDate);
            else if (datesValid && (expense.Date < request.StartDate || expense.Date > request.EndDate)) errors.Add("expense.date", ExpenseDatesWithin);
        }
        // BR3.5: with None, other expense fields are ignored and never validated.
    }
}
