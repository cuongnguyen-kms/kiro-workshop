using ContractManagement.Api.Contracts;
using ContractManagement.Api.Services;
using ContractManagement.Api.Validation;
using ContractManagement.Scheduling;
using Microsoft.Extensions.Logging.Abstractions;

namespace ContractManagement.Api.Tests;

/// <summary>Service layer with a fake engine and a fake repository (AC6.1.1, AC6.1.2, AC2.1.5, AC3.1.x, BR1.3, BR5.4).</summary>
public sealed class ContractServiceTests
{
    private readonly FakeGenerator _generator = new();
    private readonly FakeRepository _repository = new();

    private ContractService Service(IScheduleGenerator? generator = null) =>
        new(generator ?? _generator, _repository, new FixedClock(), NullLogger<ContractService>.Instance);

    [Fact]
    public async Task A_valid_contract_is_generated_once_and_saved()
    {
        var result = await Service().CreateAsync(Requests.Valid(), default);

        Assert.True(result.Succeeded);
        Assert.Equal(1, _generator.Calls);
        var saved = Assert.Single(_repository.Saved);
        Assert.Equal("Office lease", saved.Name);
        Assert.Single(saved.PaymentStreams);
        Assert.Single(saved.PaymentEntries);
        Assert.Single(saved.ExpenseEntries);
        Assert.Equal(new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero), saved.CreatedAt);
    }

    [Fact]
    public async Task An_invalid_contract_is_not_generated_or_saved()
    {
        var result = await Service().CreateAsync(Requests.Valid(name: ""), default);

        Assert.False(result.Succeeded);
        Assert.Contains("name", result.Errors!.Keys);
        Assert.Equal(0, _generator.Calls);
        Assert.Empty(_repository.Saved);
    }

    [Fact]
    public async Task The_name_is_trimmed_and_the_stream_order_is_kept()
    {
        var streams = new List<PaymentStreamRequest?>
        {
            new(10m, "Weekly", Requests.Start, new DateOnly(2027, 1, 31)),
            new(20m, "Monthly", Requests.Start, new DateOnly(2027, 3, 31)),
        };
        await Service().CreateAsync(Requests.Valid(name: "  Lease  ", streams: streams), default);

        var saved = Assert.Single(_repository.Saved);
        Assert.Equal("Lease", saved.Name);
        Assert.Equal([0, 1], saved.PaymentStreams.Select(s => s.Position));
        Assert.Equal([10m, 20m], _generator.LastRequest!.Streams.Select(s => s.Amount));
    }

    [Fact]
    public async Task A_one_time_stream_stores_its_end_date_equal_to_its_start_date()
    {
        var stream = new PaymentStreamRequest(50m, "OneTime", new DateOnly(2027, 5, 5), new DateOnly(2000, 1, 1));
        await Service().CreateAsync(Requests.Valid(streams: [stream]), default);

        var saved = Assert.Single(_repository.Saved).PaymentStreams.Single();
        Assert.Equal(new DateOnly(2027, 5, 5), saved.EndDate);
    }

    [Fact]
    public async Task Expense_fields_that_do_not_belong_to_the_method_are_not_stored()
    {
        await Service().CreateAsync(Requests.Valid(expense: new ExpenseRequest("Overtime", new DateOnly(2027, 1, 1), new DateOnly(2027, 3, 31), new DateOnly(2027, 2, 2))), default);
        await Service().CreateAsync(Requests.Valid(expense: new ExpenseRequest("AllAtOnce", new DateOnly(2027, 1, 1), new DateOnly(2027, 3, 31), new DateOnly(2027, 2, 2))), default);
        await Service().CreateAsync(Requests.Valid(expense: new ExpenseRequest("None", new DateOnly(2027, 1, 1), new DateOnly(2027, 3, 31), new DateOnly(2027, 2, 2))), default);

        var overtime = _repository.Saved[0];
        Assert.Equal((new DateOnly(2027, 1, 1), new DateOnly(2027, 3, 31), null), (overtime.ExpenseStartDate, overtime.ExpenseEndDate, overtime.ExpenseDate));
        var allAtOnce = _repository.Saved[1];
        Assert.Equal((null, null, new DateOnly(2027, 2, 2)), (allAtOnce.ExpenseStartDate, allAtOnce.ExpenseEndDate, allAtOnce.ExpenseDate));
        var none = _repository.Saved[2];
        Assert.Equal((null, null, null), (none.ExpenseStartDate, none.ExpenseEndDate, none.ExpenseDate));
        Assert.Empty(none.ExpenseEntries);
    }

    [Fact]
    public async Task Two_contracts_with_the_same_name_both_save()
    {
        await Service().CreateAsync(Requests.Valid(name: "Same"), default);
        await Service().CreateAsync(Requests.Valid(name: "Same"), default);

        Assert.Equal(2, _repository.Saved.Count);
        Assert.NotEqual(_repository.Saved[0].ContractId, _repository.Saved[1].ContractId);
    }

    [Fact]
    public async Task A_contract_over_10000_schedule_rows_is_rejected_before_the_engine_runs()
    {
        var stream = new PaymentStreamRequest(1m, "Weekly", new DateOnly(2027, 1, 1), new DateOnly(2027, 12, 31));
        var big = Requests.Valid(streams: Enumerable.Repeat<PaymentStreamRequest?>(stream, 200).ToList());

        var result = await Service().CreateAsync(big, default);

        Assert.False(result.Succeeded);
        Assert.Equal([SizeEstimator.TooLarge], result.Errors!["paymentStreams"]);
        Assert.Equal(0, _generator.Calls);
        Assert.Empty(_repository.Saved);
    }

    [Fact]
    public void The_size_estimate_never_undercounts_the_real_engine()
    {
        var generator = new ScheduleGenerator();
        var s = new DateOnly(2027, 1, 31);
        var e = new DateOnly(2029, 3, 1);
        foreach (var frequency in Enum.GetValues<Frequency>())
        {
            var end = frequency == Frequency.OneTime ? s : e;
            var stream = new StreamInput(Guid.NewGuid(), 10m, frequency, s, end);
            var request = new ScheduleRequest([stream], new ExpenseInput(ExpenseMethod.None, null, null, null));
            Assert.True(SizeEstimator.Estimate(request) >= generator.Generate(request).Payments.Count, frequency.ToString());
        }
    }

    [Fact]
    public async Task An_engine_failure_saves_nothing_and_surfaces()
    {
        _generator.Throw = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => Service().CreateAsync(Requests.Valid(), default));

        Assert.Empty(_repository.Saved);
    }

    [Fact]
    public async Task A_repository_failure_surfaces_and_nothing_is_saved()
    {
        _repository.Throw = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => Service().CreateAsync(Requests.Valid(), default));

        Assert.Empty(_repository.Saved);
    }

    [Fact]
    public async Task Engine_totals_that_break_the_rules_are_not_saved()
    {
        var result = await Assert.ThrowsAsync<InvalidOperationException>(() => Service(new BrokenTotalsGenerator()).CreateAsync(Requests.Valid(), default));

        Assert.Contains("totals", result.Message);
        Assert.Empty(_repository.Saved);
    }

    [Fact]
    public async Task With_the_real_generator_the_schedules_match_the_engine_output()
    {
        // 3 monthly payments of 100.00 and one expense row for the whole amount (AC4.1.1, AC5.1.2).
        var result = await Service(new ScheduleGenerator()).CreateAsync(Requests.Valid(), default);

        Assert.True(result.Succeeded);
        var saved = Assert.Single(_repository.Saved);
        Assert.Equal([new DateOnly(2027, 1, 1), new DateOnly(2027, 2, 1), new DateOnly(2027, 3, 1)], saved.PaymentEntries.Select(p => p.DueDate));
        Assert.Equal(300.00m, saved.PaymentEntries.Sum(p => p.Amount));
        var expense = Assert.Single(saved.ExpenseEntries);
        Assert.Equal(300.00m, expense.Amount);
        Assert.Equal(new DateOnly(2027, 3, 31), expense.PeriodStart);
    }

    private sealed class FixedClock : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2027, 1, 1, 0, 0, 0, TimeSpan.Zero);
    }

    private sealed class BrokenTotalsGenerator : IScheduleGenerator
    {
        public ScheduleResult Generate(ScheduleRequest request) =>
            new([new PaymentEntry(request.Streams[0].StreamId, request.Streams[0].StartDate, 10m)], 99m, [], 0m, []);
    }
}
