using System.Text.Json.Serialization;

namespace Transwextions.Data.Payloads;

public class TreasuryReportingResponse
{
    [JsonPropertyName("data")]
    public List<RateOfExchangeObject> Data { get; init; } = [];

    [JsonPropertyName("meta")]
    public MetaObject Meta { get; init; } = new();

    [JsonPropertyName("links")]
    public LinksObject Links { get; init; } = new();

    public class MetaObject
    {
        [JsonPropertyName("count")] 
        public int Count { get; init; }

        [JsonPropertyName("total-count")] 
        public int TotalCount { get; init; }

        [JsonPropertyName("total-pages")] 
        public int TotalPages { get; init; }
    }

    public sealed class LinksObject
    {
        [JsonPropertyName("next")] 
        public string? Next { get; init; }
    }

    public sealed class RateOfExchangeObject
    {
        [JsonPropertyName("record_date")] 
        public DateOnly RecordDate { get; init; }

        [JsonPropertyName("currency")] 
        public string Currency { get; init; } = "";

        // Exchange rate is returned as a string in the API response. (Convert to decimal)
        [JsonPropertyName("exchange_rate")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] 
        public decimal ExchangeRate { get; init; }
    }
}
