using Transwextions.Data.Models;

namespace Transwextions.App.Services.Interfaces;

public interface ITreasuryReportingRatesService
{
    public Task<ServiceResult<List<string>>> GetCurrenciesAsync(CancellationToken cancellationToken = default);

    public Task<ServiceResult<List<ExchangeRateModel>>> GetExchangeRatesByDateRangeAsync(
        DateTime MinDate,
        DateTime MaxDate,
        CancellationToken cancellationToken = default);
}