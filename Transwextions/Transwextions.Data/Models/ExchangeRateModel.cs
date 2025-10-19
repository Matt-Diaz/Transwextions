namespace Transwextions.Data.Models;

public class ExchangeRateModel
{
    public decimal ExchangeRate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateOnly RecordDate { get; set; }
}