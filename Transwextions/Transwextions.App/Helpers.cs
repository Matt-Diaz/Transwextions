namespace Transwextions.App;

public static class Helpers
{
    public static decimal ConvertCentsToDecimal(ulong cents)
    {
        decimal total = Math.Round(cents / 100m, 2);
        return total;
    }

    public static string ConvertDecimalToCurrencyString(decimal amount)
    {
        return amount.ToString("C2");
    }

    public static ulong ConvertDecimalToCents(decimal amount)
    {
        ulong cents = (ulong)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);
        return cents;
    }

    public static string ConvertCentsToCurrencyString(ulong cents)
    {
        decimal total = ConvertCentsToDecimal(cents);
        return ConvertDecimalToCurrencyString(total);
    }

    public static decimal ConverTotalCentsToDeciamlUsingExchangeRate(ulong totalCents, decimal exchangeRate)
    {
        decimal usd = (exchangeRate * totalCents) / 100m;
        usd = Math.Round(usd, 2, MidpointRounding.AwayFromZero);

        return usd;
    }
}