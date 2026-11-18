using System.Text.Json;
using System.Text.Json.Serialization;

namespace ContractManagement.Api.Contracts;

/// <summary>Body of <c>POST /api/v1/contracts</c> (contract C1). Every field is optional here so the validator can report all problems together.</summary>
public sealed record CreateContractRequest(
    string? Name,
    [property: JsonConverter(typeof(LenientDateConverter))] DateOnly? StartDate,
    [property: JsonConverter(typeof(LenientDateConverter))] DateOnly? EndDate,
    List<PaymentStreamRequest?>? PaymentStreams,
    ExpenseRequest? Expense);

/// <summary>One payment stream of the request.</summary>
public sealed record PaymentStreamRequest(
    [property: JsonConverter(typeof(LenientDecimalConverter))] decimal? Amount,
    string? Frequency,
    [property: JsonConverter(typeof(LenientDateConverter))] DateOnly? StartDate,
    [property: JsonConverter(typeof(LenientDateConverter))] DateOnly? EndDate);

/// <summary>The expense settings of the request.</summary>
public sealed record ExpenseRequest(
    string? Method,
    [property: JsonConverter(typeof(LenientDateConverter))] DateOnly? StartDate,
    [property: JsonConverter(typeof(LenientDateConverter))] DateOnly? EndDate,
    [property: JsonConverter(typeof(LenientDateConverter))] DateOnly? Date);

/// <summary>Reads a date; a missing, null, mistyped or unparsable value becomes null so the validator answers "Enter a valid date.".</summary>
public sealed class LenientDateConverter : JsonConverter<DateOnly?>
{
    /// <inheritdoc />
    public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String
            && DateOnly.TryParseExact(reader.GetString(), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var date))
        {
            return date;
        }

        JsonReaderExtensions.SkipContainer(ref reader);
        return null;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
    {
        if (value is null) writer.WriteNullValue();
        else writer.WriteStringValue(value.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture));
    }
}

/// <summary>Reads a JSON number as decimal (never double); anything else becomes null so the validator answers "Amount must be greater than zero.".</summary>
public sealed class LenientDecimalConverter : JsonConverter<decimal?>
{
    /// <inheritdoc />
    public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetDecimal(out var value))
        {
            return value;
        }

        JsonReaderExtensions.SkipContainer(ref reader);
        return null;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
    {
        if (value is null) writer.WriteNullValue();
        else writer.WriteNumberValue(value.Value);
    }
}

internal static class JsonReaderExtensions
{
    /// <summary>Moves past an object or array value. Strings, numbers and null are a single token, so there is nothing to skip.</summary>
    public static void SkipContainer(ref Utf8JsonReader reader)
    {
        if (reader.TokenType is JsonTokenType.StartObject or JsonTokenType.StartArray)
        {
            reader.Skip();
        }
    }
}
