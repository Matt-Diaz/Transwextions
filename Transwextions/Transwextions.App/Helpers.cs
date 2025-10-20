namespace Transwextions.App;

public static class Helpers
{
    /// <summary>
    /// Converts a monetary value in cents to its decimal representation in dollars, rounded to two decimal places.
    /// </summary>
    /// <param name="cents">The amount in cents to convert. Must be zero or greater.</param>
    /// <returns>A decimal value representing the equivalent dollar amount, rounded to two decimal places.</returns>
    public static decimal ConvertCentsToDecimal(ulong cents)
    {
        decimal total = Math.Round(cents / 100m, 2);
        return total;
    }

    /// <summary>
    /// Converts the specified decimal value to a currency-formatted string using the current culture.
    /// </summary>
    /// <param name="amount">The decimal value to format as a currency string.</param>
    /// <returns>A string that represents the specified amount formatted as currency with two decimal places, according to the
    /// current culture settings.</returns>
    public static string ConvertDecimalToCurrencyString(decimal amount)
    {
        return amount.ToString("C2");
    }

    /// <summary>
    /// Converts a monetary amount represented as a decimal to its equivalent value in cents
    /// </summary>
    /// <param name="amount">The decimal monetary amount to convert.</param>
    /// <returns>An unsigned 64-bit integer representing the amount in cents, rounded to the nearest cent using midpoint rounding
    /// away from zero.</returns>
    public static ulong ConvertDecimalToCents(decimal amount)
    {
        ulong cents = (ulong)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);
        return cents;
    }

    /// <summary>
    /// Converts a monetary value specified in cents to a formatted currency string.
    /// </summary>
    /// <param name="cents">The amount in cents to convert.</param>
    /// <returns>A string representing the formatted currency value corresponding to the specified number of cents.</returns>
    public static string ConvertCentsToCurrencyString(ulong cents)
    {
        decimal total = ConvertCentsToDecimal(cents);
        return ConvertDecimalToCurrencyString(total);
    }

    /// <summary>
    /// Converts a total amount in cents to its decimal value using the specified exchange rate.
    /// </summary>
    /// <remarks>The result is rounded to two decimal places.</remarks>
    /// <param name="totalCents">The total amount, in cents, to be converted.</param>
    /// <param name="exchangeRate">The exchange rate to apply when converting cents to the target currency.</param>
    /// <returns>A decimal value representing the converted amount, rounded to two decimal places using midpoint rounding away
    /// from zero.</returns>
    public static decimal ConverTotalCentsToDeciamlUsingExchangeRate(ulong totalCents, decimal exchangeRate)
    {
        decimal usd = (exchangeRate * totalCents) / 100m;
        usd = Math.Round(usd, 2, MidpointRounding.AwayFromZero);

        return usd;
    }
}