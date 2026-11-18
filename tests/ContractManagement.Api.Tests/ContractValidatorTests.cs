using ContractManagement.Api.Contracts;
using ContractManagement.Api.Validation;

namespace ContractManagement.Api.Tests;

/// <summary>Every validation rule gives its exact message and field path (BR1.1 to BR3.5, BR5.3).</summary>
public sealed class ContractValidatorTests
{
    private static readonly DateOnly D1 = new(2027, 1, 1);
    private static readonly DateOnly D2 = new(2027, 12, 31);

    private static Dictionary<string, string[]> Run(CreateContractRequest request) => ContractValidator.Validate(request).ToDictionary();

    private static void AssertOnly(Dictionary<string, string[]> errors, string path, string message)
    {
        var entry = Assert.Single(errors);
        Assert.Equal(path, entry.Key);
        Assert.Equal([message], entry.Value);
    }

    [Fact]
    public void A_valid_request_has_no_errors() => Assert.True(ContractValidator.Validate(Requests.Valid()).IsEmpty);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void A_blank_name_is_required(string? name) =>
        AssertOnly(Run(Requests.Valid() with { Name = name }), "name", "Contract name is required.");

    [Fact]
    public void A_name_over_200_characters_after_trimming_is_rejected()
    {
        AssertOnly(Run(Requests.Valid() with { Name = new string('a', 201) }), "name", "Contract name must be 200 characters or fewer.");
        Assert.True(ContractValidator.Validate(Requests.Valid() with { Name = " " + new string('a', 200) + " " }).IsEmpty);
    }

    [Fact]
    public void End_date_before_start_date_is_rejected() =>
        AssertOnly(Run(Requests.Valid() with { EndDate = new DateOnly(2026, 12, 31) }), "endDate", "End date must not be before the start date.");

    [Fact]
    public void A_missing_contract_date_asks_for_a_valid_date()
    {
        var errors = Run(Requests.Valid() with { StartDate = null });
        AssertOnly(errors, "startDate", "Enter a valid date.");
    }

    [Fact]
    public void At_least_one_stream_is_required()
    {
        AssertOnly(Run(Requests.Valid() with { PaymentStreams = [] }), "paymentStreams", "Add at least one payment stream.");
        AssertOnly(Run(Requests.Valid() with { PaymentStreams = null }), "paymentStreams", "Add at least one payment stream.");
    }

    [Theory]
    [InlineData(0, "Amount must be greater than zero.")]
    [InlineData(-5, "Amount must be greater than zero.")]
    [InlineData(10.123, "Amount can have at most 2 decimal places.")]
    public void Stream_amount_rules(double amount, string message)
    {
        var stream = new PaymentStreamRequest((decimal)amount, "Monthly", D1, D2);
        AssertOnly(Run(Requests.Valid(streams: [stream])), "paymentStreams[0].amount", message);
    }

    [Fact]
    public void A_missing_amount_is_rejected() =>
        AssertOnly(Run(Requests.Valid(streams: [new PaymentStreamRequest(null, "Monthly", D1, D2)])), "paymentStreams[0].amount", "Amount must be greater than zero.");

    [Theory]
    [InlineData(null)]
    [InlineData("monthly")]
    [InlineData("Daily")]
    [InlineData("1")]
    public void Frequency_must_be_one_of_the_five_values(string? frequency) =>
        AssertOnly(Run(Requests.Valid(streams: [new PaymentStreamRequest(10m, frequency, D1, D2)])), "paymentStreams[0].frequency", "Choose a frequency.");

    [Fact]
    public void Stream_dates_must_be_within_the_contract_dates()
    {
        var early = new PaymentStreamRequest(10m, "Monthly", new DateOnly(2026, 12, 1), D2);
        var late = new PaymentStreamRequest(10m, "Monthly", D1, new DateOnly(2028, 1, 1));
        var errors = Run(Requests.Valid(streams: [early, late]));
        Assert.Equal(["Stream dates must be within the contract dates."], errors["paymentStreams[0].startDate"]);
        Assert.Equal(["Stream dates must be within the contract dates."], errors["paymentStreams[1].endDate"]);
    }

    [Fact]
    public void Stream_date_checks_are_skipped_when_the_contract_dates_are_invalid()
    {
        var request = Requests.Valid() with { EndDate = new DateOnly(2026, 1, 1) };
        AssertOnly(Run(request), "endDate", "End date must not be before the start date.");
    }

    [Fact]
    public void A_repeating_stream_needs_an_end_date_not_before_its_start()
    {
        var missing = new PaymentStreamRequest(10m, "Weekly", D1, null);
        var backwards = new PaymentStreamRequest(10m, "Weekly", new DateOnly(2027, 6, 1), new DateOnly(2027, 5, 1));
        var errors = Run(Requests.Valid(streams: [missing, backwards]));
        Assert.Equal(["Enter a valid date."], errors["paymentStreams[0].endDate"]);
        Assert.Equal(["End date must not be before the start date."], errors["paymentStreams[1].endDate"]);
    }

    [Fact]
    public void A_one_time_stream_needs_no_end_date_and_ignores_a_submitted_one()
    {
        var noEnd = new PaymentStreamRequest(10m, "OneTime", D1, null);
        var junkEnd = new PaymentStreamRequest(10m, "OneTime", D1, new DateOnly(2000, 1, 1));
        Assert.True(ContractValidator.Validate(Requests.Valid(streams: [noEnd, junkEnd])).IsEmpty);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Spread")]
    public void Expense_method_must_be_one_of_the_three_values(string? method) =>
        AssertOnly(Run(Requests.Valid(expense: new ExpenseRequest(method, null, null, null))), "expense.method", "Choose an expense method.");

    [Fact]
    public void A_missing_expense_object_asks_for_a_method() =>
        AssertOnly(Run(Requests.Valid() with { Expense = null }), "expense.method", "Choose an expense method.");

    [Fact]
    public void Overtime_needs_both_dates_and_an_end_not_before_the_start()
    {
        var missing = Run(Requests.Valid(expense: new ExpenseRequest("Overtime", null, null, null)));
        Assert.Equal(["Enter a valid date."], missing["expense.startDate"]);
        Assert.Equal(["Enter a valid date."], missing["expense.endDate"]);

        var backwards = Run(Requests.Valid(expense: new ExpenseRequest("Overtime", new DateOnly(2027, 6, 1), new DateOnly(2027, 5, 1), null)));
        Assert.Equal(["Expense end date must not be before the start date."], backwards["expense.endDate"]);
    }

    [Fact]
    public void All_at_once_needs_a_date() =>
        AssertOnly(Run(Requests.Valid(expense: new ExpenseRequest("AllAtOnce", null, null, null))), "expense.date", "Enter a valid date.");

    [Fact]
    public void Expense_dates_must_be_within_the_contract_dates()
    {
        var overtime = Run(Requests.Valid(expense: new ExpenseRequest("Overtime", new DateOnly(2026, 1, 1), new DateOnly(2028, 1, 1), null)));
        Assert.Equal(["Expense dates must be within the contract dates."], overtime["expense.startDate"]);
        Assert.Equal(["Expense dates must be within the contract dates."], overtime["expense.endDate"]);

        AssertOnly(Run(Requests.Valid(expense: new ExpenseRequest("AllAtOnce", null, null, new DateOnly(2028, 1, 1)))),
            "expense.date", "Expense dates must be within the contract dates.");
    }

    [Fact]
    public void With_method_none_other_expense_fields_are_never_validated() =>
        Assert.True(ContractValidator.Validate(Requests.Valid(expense: new ExpenseRequest("None", new DateOnly(1999, 1, 1), null, new DateOnly(1999, 1, 1)))).IsEmpty);

    [Fact]
    public void Every_problem_is_returned_together()
    {
        var errors = Run(new CreateContractRequest(null, D1, D2, [new PaymentStreamRequest(0m, null, null, null)], null));
        Assert.Contains("name", errors.Keys);
        Assert.Contains("paymentStreams[0].amount", errors.Keys);
        Assert.Contains("paymentStreams[0].frequency", errors.Keys);
        Assert.Contains("paymentStreams[0].startDate", errors.Keys);
        Assert.Contains("expense.method", errors.Keys);
    }

    [Theory]
    [InlineData(0, 10, "page", "Page must be 1 or more.")]
    [InlineData(1, 25, "pageSize", "Page size must be 10, 20, 50 or 100.")]
    public void Paging_rules(int page, int pageSize, string path, string message) =>
        AssertOnly(ContractValidator.ValidatePaging(page, pageSize).ToDictionary(), path, message);
}
