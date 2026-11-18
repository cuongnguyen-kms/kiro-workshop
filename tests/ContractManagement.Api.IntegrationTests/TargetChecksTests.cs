using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ContractManagement.Api.Domain;
using ContractManagement.Api.Persistence;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ContractManagement.Api.IntegrationTests;

/// <summary>
/// Build and Test checks for the measurable NFR targets of u2-contract-api that the unit tests do not time:
/// NFR5.2 (create at the size cap), NFR5.3 (read), NFR5.4 (list with 10,000 contracts), NFR5.7 (10 concurrent creates)
/// and NFR6.3 (256 KB body limit). Limits are the ones in the NFR requirements and are never loosened here.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class TargetChecksTests : IAsyncLifetime
{
    private ApiFactory _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await TestDatabase.ResetAsync();
        _factory = new ApiFactory();
        _client = _factory.CreateClient();
        // Warm-up request, so the first-call start-up cost is not counted.
        await _client.GetAsync("/api/v1/contracts");
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    private static StringContent Json(string body) => new(body, Encoding.UTF8, "application/json");

    private static string WeeklyBody(string name, string start, string end) => $$"""
        {
          "name": "{{name}}",
          "startDate": "{{start}}",
          "endDate": "{{end}}",
          "paymentStreams": [ { "amount": 10.00, "frequency": "Weekly", "startDate": "{{start}}", "endDate": "{{end}}" } ],
          "expense": { "method": "None" }
        }
        """;

    [Fact]
    public async Task NFR5_2_and_NFR5_3_create_near_the_size_cap_in_5_seconds_and_read_back_in_500_ms()
    {
        // About 9,860 weekly payments, just under the 10,000 row cap.
        var body = WeeklyBody("Cap contract", "1900-01-01", "2089-01-01");

        var timer = Stopwatch.StartNew();
        var created = await _client.PostAsync("/api/v1/contracts", Json(body));
        timer.Stop();
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.True(timer.Elapsed < TimeSpan.FromSeconds(5), $"Create took {timer.ElapsedMilliseconds} ms (target 5000 ms).");

        var detail = await created.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(detail.GetProperty("paymentSchedule").GetProperty("entries").GetArrayLength() > 9000);

        // Warm the read path once on a small contract, so query compilation is not counted.
        var small = await _client.PostAsync("/api/v1/contracts", Json(WeeklyBody("Warm-up", "2027-01-01", "2027-02-01")));
        await _client.GetAsync(small.Headers.Location);

        timer.Restart();
        var read = await _client.GetAsync(created.Headers.Location);
        var text = await read.Content.ReadAsStringAsync();
        timer.Stop();
        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
        Assert.NotEmpty(text);
        Assert.True(timer.Elapsed < TimeSpan.FromMilliseconds(500), $"Read took {timer.ElapsedMilliseconds} ms (target 500 ms).");
    }

    [Fact]
    public async Task NFR5_4_list_one_page_in_300_ms_with_10000_saved_contracts()
    {
        await using (var db = TestDatabase.CreateContext())
        {
            var contracts = Enumerable.Range(0, 10_000)
                .Select(i => ContractFactory.Create($"Contract {i:D5}", new DateOnly(2000, 1, 1).AddDays(i % 3000)))
                .ToList();
            db.Contracts.AddRange(contracts);
            await db.SaveChangesAsync();
            Assert.Equal(10_000, await db.Contracts.CountAsync());
        }

        await _client.GetAsync("/api/v1/contracts?page=1&pageSize=10"); // warm-up for this data
        var timer = Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/v1/contracts?page=2&pageSize=10");
        var page = await response.Content.ReadFromJsonAsync<JsonElement>();
        timer.Stop();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(10_000, page.GetProperty("totalCount").GetInt32());
        Assert.Equal(10, page.GetProperty("items").GetArrayLength());
        Assert.True(timer.Elapsed < TimeSpan.FromMilliseconds(300), $"List took {timer.ElapsedMilliseconds} ms (target 300 ms).");
    }

    [Fact]
    public async Task NFR5_7_ten_simultaneous_creates_all_succeed_and_stay_separate()
    {
        var calls = Enumerable.Range(0, 10)
            .Select(i => _client.PostAsync("/api/v1/contracts", Json(WeeklyBody($"Parallel {i}", "2027-01-01", "2027-06-30"))))
            .ToArray();
        var responses = await Task.WhenAll(calls);

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.Created, r.StatusCode));
        var ids = new HashSet<Guid>();
        foreach (var r in responses)
        {
            var detail = await r.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(26, detail.GetProperty("paymentSchedule").GetProperty("entries").GetArrayLength());
            ids.Add(detail.GetProperty("id").GetGuid());
        }
        Assert.Equal(10, ids.Count);

        var list = await (await _client.GetAsync("/api/v1/contracts?pageSize=100")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(10, list.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task NFR6_4_an_unexpected_failure_returns_a_generic_500_with_no_internal_detail_and_saves_nothing()
    {
        const string secretDetail = "SECRET-SQL select * from contracts; Host=db-internal; Password=hunter2";
        using var failing = _factory.WithWebHostBuilder(b => b.ConfigureTestServices(services =>
        {
            services.RemoveAll<IContractRepository>();
            services.AddScoped<IContractRepository>(_ => new ThrowingRepository(secretDetail));
        }));
        using var client = failing.CreateClient();

        var response = await client.PostAsync("/api/v1/contracts", Json(WeeklyBody("Will fail", "2027-01-01", "2027-03-31")));
        var text = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = JsonDocument.Parse(text).RootElement;
        Assert.Equal(500, problem.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(problem.GetProperty("traceId").GetString()));
        Assert.DoesNotContain("SECRET-SQL", text);
        Assert.DoesNotContain("Password", text);
        Assert.DoesNotContain("hunter2", text);
        Assert.DoesNotContain("ThrowingRepository", text);
        Assert.DoesNotContain(" at ", text);

        var list = await (await _client.GetAsync("/api/v1/contracts")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0, list.GetProperty("totalCount").GetInt32());
    }

    private sealed class ThrowingRepository(string message) : IContractRepository
    {
        public Task SaveAsync(Contract contract, CancellationToken cancellationToken) => throw new InvalidOperationException(message);
        public Task<Contract?> GetAsync(Guid contractId, CancellationToken cancellationToken) => throw new InvalidOperationException(message);
        public Task<ContractListResult> ListAsync(int page, int pageSize, CancellationToken cancellationToken) => throw new InvalidOperationException(message);
    }

    [Fact]
    public async Task NFR6_3_a_body_over_256_KB_is_rejected_with_413_by_a_real_Kestrel_server()
    {
        using var kestrel = new ApiFactory();
        kestrel.UseKestrel();
        kestrel.StartServer();
        using var client = kestrel.CreateClient();

        var padding = new string('x', 300 * 1024);
        var response = await client.PostAsync("/api/v1/contracts", Json($$"""{ "name": "{{padding}}" }"""));

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
    }
}
