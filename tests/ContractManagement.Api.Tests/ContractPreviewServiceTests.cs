using ContractManagement.Api.Contracts;
using ContractManagement.Api.Services;
using ContractManagement.Api.Validation;
using ContractManagement.Scheduling;
using Microsoft.Extensions.Logging.Abstractions;

namespace ContractManagement.Api.Tests;

/// <summary>Preview in the service layer: real generator, fake repository (FR2.1 to FR2.7, NFR2).</summary>
public sealed class ContractPreviewServiceTests
{
    private readonly FakeRepository _repository = new();

    private ContractService Service() =>
        new(new ScheduleGenerator(), _repository, TimeProvider.System, NullLogger<ContractService>.Instance);

    [Fact]
    public async Task Preview_equals_what_create_returns_for_the_same_input()
    {
        var request = Requests.Valid(streams:
        [
            new PaymentStreamRequest(1000m, "Monthly", Requests.Start, new DateOnly(2027, 3, 31)),
            new PaymentStreamRequest(50m, "OneTime", new DateOnly(2027, 5, 5), null),
        ]);

        var preview = Service().Preview(request, PreviewKind.Expense);
        var created = await Service().CreateAsync(request, default);

        Assert.True(preview.Succeeded);
        var contract = ContractMapper.ToDetail(created.Contract!);
        Assert.Equal(
            contract.PaymentSchedule.Entries.Select(e => (e.DueDate, e.Amount)),
            preview.Preview!.PaymentSchedule.Entries.Select(e => (e.DueDate, e.Amount)));
        Assert.Equal(contract.PaymentSchedule.Total, preview.Preview.PaymentSchedule.Total);
        Assert.Equal(contract.ExpenseSchedule.Entries, preview.Preview.ExpenseSchedule.Entries);
        Assert.Equal(contract.ExpenseSchedule.Total, preview.Preview.ExpenseSchedule.Total);
        Assert.Contains(preview.Preview.PaymentSchedule.Entries, e => e.StreamNumber == 2);
    }

    [Fact]
    public void Preview_never_touches_the_repository()
    {
        Service().Preview(Requests.Valid(), PreviewKind.Expense);

        Assert.Empty(_repository.Saved);
    }

    [Fact]
    public void Payment_preview_ignores_invalid_expense_fields()
    {
        var expense = new ExpenseRequest("Overtime", null, null, null);

        var result = Service().Preview(Requests.Valid(expense: expense), PreviewKind.Payment);

        Assert.True(result.Succeeded);
        Assert.Empty(result.Preview!.ExpenseSchedule.Entries);
        Assert.NotEmpty(result.Preview.PaymentSchedule.Entries);
    }

    [Fact]
    public void Preview_with_a_blank_name_succeeds()
    {
        Assert.True(Service().Preview(Requests.Valid(name: ""), PreviewKind.Expense).Succeeded);
    }

    [Fact]
    public async Task An_invalid_stream_gives_the_same_message_and_path_as_create()
    {
        var bad = Requests.Valid(streams: [new PaymentStreamRequest(0m, "Monthly", Requests.Start, new DateOnly(2027, 3, 31))]);

        var preview = Service().Preview(bad, PreviewKind.Payment);
        var created = await Service().CreateAsync(bad, default);

        Assert.False(preview.Succeeded);
        Assert.Equal(created.Errors!["paymentStreams[0].amount"], preview.Errors!["paymentStreams[0].amount"]);
    }

    [Fact]
    public void A_preview_over_10000_rows_is_rejected_with_the_create_message()
    {
        var request = Requests.Valid() with
        {
            StartDate = new DateOnly(1000, 1, 1),
            EndDate = new DateOnly(2999, 12, 31),
            PaymentStreams = [new PaymentStreamRequest(1m, "Weekly", new DateOnly(1000, 1, 1), new DateOnly(2999, 12, 31))],
        };

        var result = Service().Preview(request, PreviewKind.Payment);

        Assert.False(result.Succeeded);
        Assert.Equal(SizeEstimator.TooLarge, result.Errors!["paymentStreams"].Single());
    }
}
