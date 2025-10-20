using System.Text.Json;
using Transwextions.App.Services.Interfaces;
using Transwextions.Data.Constants;
using Transwextions.Data.Models;
using Transwextions.Data.Payloads;

namespace Transwextions.App.Services;

public class TreasuryReportingRatesService : ITreasuryReportingRatesService
{
    protected readonly HttpClient _httpClient;
    protected const int MAX_PAGE_LOOPS = 100;

    public TreasuryReportingRatesService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Asynchronously retrieves the list of available currency codes from the U.S. Treasury Reporting Rates of Exchange API.
    /// </summary>
    /// <remarks>The returned list contains unique currency codes in alphabetical order. If the operation
    /// fails or no currencies are found, the ServiceResult will indicate failure and include an error message.</remarks>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>Returns a ServiceResult with a list ofbcurrency codes as strings, or a failure result if the currencies could not be retrieved.</returns>
    public async Task<ServiceResult<List<string>>> GetCurrenciesAsync(
     CancellationToken cancellationToken = default)
    {
        var baseUrl = TreasuryReportingRatesAPIEndpoints.GetAllCurrenciesEndpoint;
        var currencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var loopCount = 0;
        var requestedUrls = new List<string>();
        string? currentUrl = baseUrl;

        try
        {
            while (!string.IsNullOrEmpty(currentUrl))
            {
                if (loopCount >= MAX_PAGE_LOOPS)
                    return ServiceResult<List<string>>.Failure("Exceeded maximum page loop limit: " + MAX_PAGE_LOOPS.ToString());

                loopCount++;

                if (requestedUrls.Contains(currentUrl))
                    continue;

                using var response = await _httpClient.GetAsync(currentUrl, cancellationToken);

                requestedUrls.Add(currentUrl);

                if (!response.IsSuccessStatusCode)
                    return ServiceResult<List<string>>.Failure($"HTTP {(int)response.StatusCode}");

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

                var payload = await JsonSerializer.DeserializeAsync<TreasuryReportingResponse>(stream, cancellationToken: cancellationToken);

                if (payload == null)
                    return ServiceResult<List<string>>.Failure("Failed to deserialize Treasury response.");

                if(payload.Data == null)
                    return ServiceResult<List<string>>.Failure("Treasury response contained no data.");

                // Grab currency data from payload.
                foreach (var row in payload.Data)
                    if (!string.IsNullOrWhiteSpace(row.Currency))
                        currencies.Add(row.Currency);

                var nextLink = payload.Links?.Next;

                // Set next url - Null if there is no next link.
                currentUrl = string.IsNullOrWhiteSpace(nextLink)
                    ? null
                    : baseUrl + nextLink;
            }

            var result = currencies.OrderBy(x => x).ToList();

            return result.Count == 0
                ? ServiceResult<List<string>>.Failure("No currencies returned.")
                : ServiceResult<List<string>>.Success(result);
        }
        catch (OperationCanceledException)
        {
            return ServiceResult<List<string>>.Failure($"Error retrieving currencies: Operation canceled.");
        }
        catch(JsonException ex)
        {
            return ServiceResult<List<string>>.Failure($"Failed to deserialize Treasury response: " + ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<string>>.Failure($"Error retrieving currencies: {ex.Message}");
        }
    }

    /// <summary>
    /// Asynchronously retrieves the list of available exchange rate data from the U.S. Treasury Reporting Rates of Exchange API within a date range.
    /// </summary>
    /// <param name="MaxDate"></param>
    /// <param name="MinDate"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<ServiceResult<List<ExchangeRateModel>>> GetExchangeRatesByDateRangeAsync(
     DateTime MinDate,
     DateTime MaxDate,
     CancellationToken cancellationToken = default)
    {
        if(MinDate > MaxDate)
            return ServiceResult<List<ExchangeRateModel>>.Failure("MinDate cannot be greater than MaxDate.");

        var minDateFormatted = MinDate.ToString("yyyy-MM-dd");
        var maxDateFormatted = MaxDate.ToString("yyyy-MM-dd");
        var baseUrl = string.Format(TreasuryReportingRatesAPIEndpoints.GetAllRatesWithinDateRangeEndpoint, minDateFormatted, maxDateFormatted);
        var exchangeRates = new HashSet<ExchangeRateModel>();
        var loopCount = 0;
        var requestedUrls = new List<string>();
        string? currentUrl = baseUrl;

        try
        {
            while (!string.IsNullOrEmpty(currentUrl))
            {
                if (loopCount >= MAX_PAGE_LOOPS)
                    return ServiceResult<List<ExchangeRateModel>>.Failure("Exceeded maximum page loop limit: " + MAX_PAGE_LOOPS.ToString());

                loopCount++;

                if (requestedUrls.Contains(currentUrl))
                    continue;

                using var response = await _httpClient.GetAsync(currentUrl, cancellationToken);

                requestedUrls.Add(currentUrl);

                if (!response.IsSuccessStatusCode)
                    return ServiceResult<List<ExchangeRateModel>>.Failure($"HTTP {(int)response.StatusCode}");

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

                var payload = await JsonSerializer.DeserializeAsync<TreasuryReportingResponse>(stream, cancellationToken: cancellationToken);

                if (payload == null)
                    return ServiceResult<List<ExchangeRateModel>>.Failure("Failed to deserialize Treasury response.");

                // Grab exchange rate data from payload.
                foreach (var row in payload.Data)
                {
                    var exchangeRate = new ExchangeRateModel()
                    {
                        CountryCurrency = row.Currency,
                        ExchangeRate = row.ExchangeRate,
                        RecordDate = row.RecordDate
                    };

                    exchangeRates.Add(exchangeRate);
                }

                var nextLink = payload.Links?.Next;

                // Set next url - Null if there is no next link.
                currentUrl = string.IsNullOrWhiteSpace(nextLink)
                    ? null
                    : baseUrl + nextLink;
            }

            var result = exchangeRates.OrderBy(x => x.CountryCurrency).ToList();

            return result.Count == 0
                ? ServiceResult<List<ExchangeRateModel>>.Failure("No exchange rate data returned.")
                : ServiceResult<List<ExchangeRateModel>>.Success(result);
        }
        catch (OperationCanceledException)
        {
            return ServiceResult<List<ExchangeRateModel>>.Failure($"Error retrieving exchange rate data: Operation canceled.");
        }
        catch (JsonException ex)
        {
            return ServiceResult<List<ExchangeRateModel>>.Failure($"Failed to deserialize Treasury response: " + ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<ExchangeRateModel>>.Failure($"Error retrieving exchange rate data: {ex.Message}");
        }
    }
}