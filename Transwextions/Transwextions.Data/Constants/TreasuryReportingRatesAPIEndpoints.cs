namespace Transwextions.Data.Constants;

public static class TreasuryReportingRatesAPIEndpoints
{
   /// <summary>
   /// Represents the endpoint URL format string for retrieving all currency exchange rates.
   /// </summary>
    public const string GetAllCurrenciesEndpoint = "https://api.fiscaldata.treasury.gov/services/api/fiscal_service/v1/accounting/od/rates_of_exchange?fields=currency";

    /// <summary>
    /// Represents the endpoint URL format string for retrieving all rates of exchange within a specified date range.
    /// </summary>
    /// <remarks>The format string requires two parameters: the start date, and end date.
    /// Dates should be provided in format (yyyy-MM-dd). Use string.Format to insert the appropriate values when
    /// constructing the endpoint URL.</remarks>
    public const string GetAllRatesWithinDateRangeEndpoint = "https://api.fiscaldata.treasury.gov/services/api/fiscal_service/v1/accounting/od/rates_of_exchange?filter=record_date:gte:{0},lte:{1}&fields=currency,record_date,exchange_rate";
}