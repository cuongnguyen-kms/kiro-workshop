using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ContractManagement.Api.IntegrationTests;

/// <summary>Starts the real API against <c>contract_management_test</c>.</summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public const string AllowedOrigin = "http://localhost:5173";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:ContractDbDatabase"] = TestDatabase.Name,
            ["ConnectionStrings:ContractDbPassword"] = TestDatabase.Configuration["ConnectionStrings:ContractDbPassword"],
            ["Cors:AllowedOrigin"] = AllowedOrigin,
        }));
    }
}

/// <summary>Endpoint layer: create, read, list, CORS, health, speed and the OpenAPI document.</summary>
[Collection(DatabaseCollection.Name)]
public sealed class ApiTests : IAsyncLifetime
{
    private ApiFactory _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await TestDatabase.ResetAsync();
        _factory = new ApiFactory();
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    private const string ValidBody = """
        {
          "name": "Office lease",
          "startDate": "2027-01-01",
          "endDate": "2027-12-31",
          "paymentStreams": [
            { "amount": 100.00, "frequency": "Monthly", "startDate": "2027-01-01", "endDate": "2027-03-31" }
          ],
          "expense": { "method": "AllAtOnce", "date": "2027-03-31" }
        }
        """;

    private static StringContent Json(string body) => new(body, Encoding.UTF8, "application/json");

    [Fact]
    public async Task Create_returns_201_with_both_schedules_and_a_location_header()
    {
        var response = await _client.PostAsync("/api/v1/contracts", Json(ValidBody));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var id = body.GetProperty("id").GetGuid();
        Assert.EndsWith($"/api/v1/contracts/{id}", response.Headers.Location!.ToString());
        Assert.Equal(3, body.GetProperty("paymentSchedule").GetProperty("entries").GetArrayLength());
        Assert.Equal(300.00m, body.GetProperty("paymentSchedule").GetProperty("total").GetDecimal());
        Assert.Equal(1, body.GetProperty("expenseSchedule").GetProperty("entries").GetArrayLength());
        Assert.Equal(300.00m, body.GetProperty("expenseSchedule").GetProperty("total").GetDecimal());
        Assert.Equal("Monthly", body.GetProperty("paymentStreams")[0].GetProperty("frequency").GetString());
        Assert.Equal("2027-01-01", body.GetProperty("startDate").GetString());

        var read = await _client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
        var saved = await read.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(id, saved.GetProperty("id").GetGuid());
        Assert.Equal(3, saved.GetProperty("paymentSchedule").GetProperty("entries").GetArrayLength());
    }

    [Fact]
    public async Task An_invalid_body_returns_400_with_messages_by_field_path_and_saves_nothing()
    {
        var response = await _client.PostAsync("/api/v1/contracts", Json("""
            { "name": "", "startDate": "2027-01-01", "endDate": "2027-12-31",
              "paymentStreams": [ { "amount": 0, "frequency": "Monthly", "startDate": "2027-01-01", "endDate": "2027-03-31" } ],
              "expense": { "method": "None" } }
            """));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        var errors = problem.GetProperty("errors");
        Assert.Equal("Contract name is required.", errors.GetProperty("name")[0].GetString());
        Assert.Equal("Amount must be greater than zero.", errors.GetProperty("paymentStreams[0].amount")[0].GetString());

        var list = await (await _client.GetAsync("/api/v1/contracts")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0, list.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task An_unreadable_body_returns_400_problem_details()
    {
        var response = await _client.PostAsync("/api/v1/contracts", Json("{ not json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(400, problem.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task An_invalid_date_text_asks_for_a_valid_date()
    {
        var response = await _client.PostAsync("/api/v1/contracts", Json(ValidBody.Replace("\"2027-12-31\"", "\"31/12/2027\"")));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errors");
        Assert.Equal("Enter a valid date.", errors.GetProperty("endDate")[0].GetString());
    }

    [Fact]
    public async Task Get_of_an_unknown_id_returns_404()
    {
        var response = await _client.GetAsync($"/api/v1/contracts/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Contract not found.", problem.GetProperty("title").GetString());

        var notGuid = await _client.GetAsync("/api/v1/contracts/abc");
        Assert.Equal(HttpStatusCode.NotFound, notGuid.StatusCode);
    }

    [Fact]
    public async Task List_returns_the_page_shape_with_an_empty_first_page()
    {
        var response = await _client.GetAsync("/api/v1/contracts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0, page.GetProperty("items").GetArrayLength());
        Assert.Equal(1, page.GetProperty("page").GetInt32());
        Assert.Equal(10, page.GetProperty("pageSize").GetInt32());
        Assert.Equal(0, page.GetProperty("totalCount").GetInt32());
        Assert.Equal(0, page.GetProperty("totalPages").GetInt32());
    }

    [Fact]
    public async Task List_returns_saved_contracts_and_rejects_a_bad_page_size()
    {
        await _client.PostAsync("/api/v1/contracts", Json(ValidBody));

        var page = await (await _client.GetAsync("/api/v1/contracts?page=1&pageSize=10")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, page.GetProperty("totalCount").GetInt32());
        Assert.Equal("Office lease", page.GetProperty("items")[0].GetProperty("name").GetString());

        var bad = await _client.GetAsync("/api/v1/contracts?page=0&pageSize=25");
        Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);
        var errors = (await bad.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errors");
        Assert.Equal("Page must be 1 or more.", errors.GetProperty("page")[0].GetString());
        Assert.Equal("Page size must be 10, 20, 50 or 100.", errors.GetProperty("pageSize")[0].GetString());
    }

    [Fact]
    public async Task Only_the_configured_origin_gets_a_cors_header()
    {
        using var allowed = new HttpRequestMessage(HttpMethod.Get, "/api/v1/contracts");
        allowed.Headers.Add("Origin", ApiFactory.AllowedOrigin);
        var allowedResponse = await _client.SendAsync(allowed);
        Assert.Equal(ApiFactory.AllowedOrigin, allowedResponse.Headers.GetValues("Access-Control-Allow-Origin").Single());

        using var other = new HttpRequestMessage(HttpMethod.Get, "/api/v1/contracts");
        other.Headers.Add("Origin", "http://evil.example");
        var otherResponse = await _client.SendAsync(other);
        Assert.False(otherResponse.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Health_returns_200_and_responses_carry_security_headers()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task Create_with_10_streams_over_10_years_responds_within_2_seconds()
    {
        var stream = """{ "amount": 10.00, "frequency": "Weekly", "startDate": "2027-01-01", "endDate": "2036-12-31" }""";
        var body = $$"""
            { "name": "Big", "startDate": "2027-01-01", "endDate": "2036-12-31",
              "paymentStreams": [ {{string.Join(",", Enumerable.Repeat(stream, 10))}} ],
              "expense": { "method": "Overtime", "startDate": "2027-01-01", "endDate": "2036-12-31" } }
            """;
        // Warm up the host (first request pays one-time start-up costs), then measure a normal request.
        await _client.PostAsync("/api/v1/contracts", Json(ValidBody));

        var clock = Stopwatch.StartNew();
        var response = await _client.PostAsync("/api/v1/contracts", Json(body));
        clock.Stop();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(clock.Elapsed < TimeSpan.FromSeconds(2), $"Took {clock.Elapsed.TotalMilliseconds:F0} ms");
    }

    [Fact]
    public async Task The_generated_openapi_paths_match_the_contract_summary()
    {
        var document = await _client.GetFromJsonAsync<JsonElement>("/openapi/v1.json");
        var paths = document.GetProperty("paths");

        var contracts = paths.GetProperty("/api/v1/contracts");
        Assert.Equal("createContract", contracts.GetProperty("post").GetProperty("operationId").GetString());
        Assert.Equal("listContracts", contracts.GetProperty("get").GetProperty("operationId").GetString());
        var byId = paths.GetProperty("/api/v1/contracts/{id}");
        Assert.Equal("getContract", byId.GetProperty("get").GetProperty("operationId").GetString());
        Assert.Equal(3, paths.EnumerateObject().Count());
        var preview = paths.GetProperty("/api/v1/contracts/preview");
        Assert.Equal("previewContractSchedule", preview.GetProperty("post").GetProperty("operationId").GetString());
    }
}
