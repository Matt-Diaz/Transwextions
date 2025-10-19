using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Threading.Tasks;
using Transwextions.App.Services;
using Transwextions.Data.Constants;
using Transwextions.Data.Models;

namespace Transwextions.App.Components.Modals;

public partial class ViewTransactionComponent
{
    [Parameter]
    public TransactionModel? Transaction { get; set; }

    [Parameter]
    public List<string> CurrenciesData { get; set; } = new();

    protected readonly TreasuryReportingRatesService _treasuryReportingRatesService;

    public ViewTransactionComponent(TreasuryReportingRatesService treasuryReportingRatesService)
    {
        _treasuryReportingRatesService = treasuryReportingRatesService;
    }

    public string SelectedCurrency { get; set; } = "U.S. Dollar";
    public decimal? ExchangeRate { get; set; }
    public DateOnly ExchangeRecordDate { get; set; }
    public List<ExchangeRateModel>? ExchangeRatesData { get; set; }
    public DateTime TransactionDateOverride { get; set; }
    public bool IsLoadingConversionData { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            TransactionDateOverride = Transaction?.TransactionDateUtc ?? DateTime.UtcNow;
            ExchangeRatesData = await LoadExchangeRatesData();
        }
    }

    public async Task<List<ExchangeRateModel>?> LoadExchangeRatesData()
    {
        IsLoadingConversionData = true;

        if (Transaction == null)
        {
            _notificationService.Notify(Radzen.NotificationSeverity.Error, "Transaction was not found.");
            ExchangeRate = null;
            IsLoadingConversionData = false;
            return null;
        }
        var transactionDate = TransactionDateOverride.Date;
        var minDate = transactionDate.AddMonths(-6);
        var maxDate = transactionDate;
        var exchangeRatesResult = await _treasuryReportingRatesService.GetExchangeRatesByDateRangeAsync(minDate, maxDate);

        if (exchangeRatesResult.IsSuccess == false)
        {
            _notificationService.Notify(Radzen.NotificationSeverity.Error, exchangeRatesResult.ErrorMessage);
            ExchangeRate = null;
            IsLoadingConversionData = false;
            return null;
        }

        if (exchangeRatesResult.Object == null)
        {
            _notificationService.Notify(Radzen.NotificationSeverity.Error, "There was an error loading the Exchange Rate data.");
            ExchangeRate = null;
            IsLoadingConversionData = false;
            return null;
        }

        IsLoadingConversionData = false;

        return exchangeRatesResult.Object.OrderBy(p => p.RecordDate).ToList();
    }

    private async void TransactionDateOverride_OnChange(DateTime value)
    {
        TransactionDateOverride = value;
        ExchangeRatesData = await LoadExchangeRatesData();
        await ApplyCurrencyConversion(SelectedCurrency);
        await InvokeAsync(StateHasChanged);
    }

    public async void CurrencyDropdown_OnChange(string value)
    {
        await ApplyCurrencyConversion(value);
    }

    public async Task ApplyCurrencyConversion(string currency)
    {
        SelectedCurrency = currency;

        if (SelectedCurrency == "U.S. Dollar")
        {
            ExchangeRate = null;
            return;
        }


        if (ExchangeRatesData != null && Transaction != null)
        {
            var exchange = ExchangeRatesData.FirstOrDefault(p => p.CountryCurrency == SelectedCurrency);

            if (exchange != null)
            {
                ExchangeRate = exchange.ExchangeRate;
                ExchangeRecordDate = exchange.RecordDate;
            }
            else
            {
                ExchangeRate = null;
                _notificationService.Notify(
                    Radzen.NotificationSeverity.Error,
                    "No conversion rate within 6 months exists for this currency.",
                    string.Empty,
                    TimeSpan.FromSeconds(5));
            }
        }
    }

    public string GetConvertedCurrencyValue()
    {
        if (Transaction != null && ExchangeRate != null)
        {
            decimal usd = Helpers.ConverTotalCentsToDeciamlUsingExchangeRate(Transaction.AmountTotalCents, ExchangeRate.Value);

            return Helpers.ConvertDecimalToCurrencyString(usd);
        }

        return "$0.00";
    }
}