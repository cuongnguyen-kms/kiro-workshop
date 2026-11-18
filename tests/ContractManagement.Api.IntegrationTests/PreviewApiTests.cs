using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ContractManagement.Api.IntegrationTests;

/// <summary>Schedule preview endpoint (FR2.1, FR2.2, FR2.5, NFR1, NFR3).</summary>
[Collection(DatabaseCollection.Name)]
public sealed class PreviewApiTests : IAsyncLifetime
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
    public async Task Preview_returns_200_with_both_schedules_and_stores_nothing()
    {
        var response = await _client.PostAsync("/api/v1/contracts/preview", Json(ValidBody));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var payments = body.GetProperty("paymentSchedule");
        Assert.Equal(3, payments.GetProperty("entries").GetArrayLength());
        Assert.Equal(1, payments.GetProperty("entries")[0].GetProperty("streamNumber").GetInt32());
        Assert.Equal(300.00m, payments.GetProperty("total").GetDecimal());
        var expense = body.GetProperty("expenseSchedule");
        Assert.Equal(1, expense.GetProperty("entries").GetArrayLength());
        Assert.Equal(300.00m, expense.GetProperty("total").GetDecimal());

        var list = await (await _client.GetAsync("/api/v1/contracts")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0, list.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task Preview_with_invalid_input_returns_400_with_messages_by_field_path()
    {
        var response = await _client.PostAsync("/api/v1/contracts/preview?kind=payment", Json("""
            { "startDate": "2027-01-01", "endDate": "2027-12-31",
              "paymentStreams": [ { "amount": 0, "frequency": "Monthly", "startDate": "2027-01-01", "endDate": "2027-03-31" } ],
              "expense": { "method": "None" } }
            """));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var errors = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errors");
        Assert.Equal("Amount must be greater than zero.", errors.GetProperty("paymentStreams[0].amount")[0].GetString());
        Assert.False(errors.TryGetProperty("name", out _));
    }

    [Fact]
    public async Task Preview_of_10_streams_over_10_years_responds_within_2_seconds()
    {
        var stream = """{ "amount": 10.00, "frequency": "Weekly", "startDate": "2027-01-01", "endDate": "2036-12-31" }""";
        var body = $$"""
            { "startDate": "2027-01-01", "endDate": "2036-12-31",
              "paymentStreams": [ {{string.Join(",", Enumerable.Repeat(stream, 10))}} ],
              "expense": { "method": "Overtime", "startDate": "2027-01-01", "endDate": "2036-12-31" } }
            """;
        // Warm up the host, then measure a normal request.
        await _client.PostAsync("/api/v1/contracts/preview", Json(ValidBody));

        var clock = Stopwatch.StartNew();
        var response = await _client.PostAsync("/api/v1/contracts/preview", Json(body));
        clock.Stop();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(clock.Elapsed < TimeSpan.FromSeconds(2), $"Took {clock.Elapsed.TotalMilliseconds:F0} ms");
    }
}
