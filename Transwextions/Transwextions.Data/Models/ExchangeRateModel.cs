namespace Transwextions.Data.Models;

public class ExchangeRateModel
{
    public decimal ExchangeRate { get; set; }
    public string CountryCurrency { get; set; } = string.Empty;
    public DateOnly RecordDate { get; set; }
}