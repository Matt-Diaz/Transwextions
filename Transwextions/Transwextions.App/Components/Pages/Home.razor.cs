using Radzen;
using Transwextions.App.Services;
using Transwextions.App.Services.Interfaces;

namespace Transwextions.App.Components.Pages;

public partial class Home
{
    protected readonly ITreasuryReportingRatesService _treasuryReportingRatesService;

    public Home(ITreasuryReportingRatesService treasuryReportingRatesService)
    {
        _treasuryReportingRatesService = treasuryReportingRatesService;
    }

    public List<string> CurrenciesData { get; set; } = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(firstRender)
        {
            CurrenciesData = await LoadCurrenciesData();

            StateHasChanged();
        }
    }

    private async Task<List<string>> LoadCurrenciesData()
    {
        var currencyResult = await _treasuryReportingRatesService.GetCurrenciesAsync();

        if (currencyResult == null)
        {
            _notificationService.Notify(NotificationSeverity.Error, "There was an error loading currencies.");
            return new();
        }

        if (currencyResult!.IsSuccess == false)
        {
            _notificationService.Notify(NotificationSeverity.Error, currencyResult.ErrorMessage);
            return new();
        }

        return currencyResult.Object ?? new();
    }
}