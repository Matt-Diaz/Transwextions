using Radzen;
using Transwextions.App.Components.Modals;
using Transwextions.App.Services;
using Transwextions.App.Services.Interfaces;
using Transwextions.Data.Models;

namespace Transwextions.App.Components.Layout;

public partial class MainLayout : IDisposable
{
    protected readonly IUserStateService _userStateService;
    protected readonly ITransactionService _transactionService;
    protected readonly IApplicationEventsService _applicationEventsService;
    protected readonly TreasuryReportingRatesService _treasuryReportingRatesService;

    public MainLayout(IUserStateService userStateService, ITransactionService transactionService, IApplicationEventsService applicationEventsService, TreasuryReportingRatesService treasuryReportingRatesService)
    {
        _userStateService = userStateService;
        _transactionService = transactionService;
        _applicationEventsService = applicationEventsService;
        _treasuryReportingRatesService = treasuryReportingRatesService;

        _userStateService.OnChange += OnUserStateChanged;
        _applicationEventsService.TransactionAdded += OnTransactionAdded;
        _applicationEventsService.TransactionDeleted += OnTransactionDeleted;
    }
    private bool IsLoggedIn { get; set; } = false;
    private bool SidebarExpanded = true;
    private string Username { get; set; } = string.Empty;
    private ulong TransactionsTotalCents { get; set; } = 0;
    private List<string> CurrenciesData { get; set; } = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Username = _userStateService.Username;
            IsLoggedIn = _userStateService.IsLoggedIn;
            TransactionsTotalCents = await GetTransactionsTotalCents();

            await ShowGetUsernameModal();
            await InvokeAsync(StateHasChanged);

            var result = await _treasuryReportingRatesService.GetCurrenciesAsync();
            if (result.IsSuccess && result.Object != null)
            {
                Console.WriteLine();
            }

            var result2 = await _treasuryReportingRatesService.GetExchangeRatesByDateRangeAsync(DateTime.Now.AddMonths(-6), DateTime.Now);
            if (result2.IsSuccess && result2.Object != null)
            {
                Console.WriteLine();
            }
        }
    }
  
    private async Task<ulong> GetTransactionsTotalCents()
    {
        var result = await _transactionService.GetTransactionsTotalCents();

        if (result.IsSuccess && result.Object != null)
        {
            return result.Object.Value;
        }

        return 0;
    }   

    private string GetWelcomeText()
    {
        var result = "Hello!";

        if (IsLoggedIn && !string.IsNullOrWhiteSpace(Username))
        {
            result = $"Hello, {Username}!";
        }

        return result;
    }

    private string GetTransactionsTotalText()
    {
        decimal total = TransactionsTotalCents / 100m;
        return total.ToString("C2");
    }

    private async void LoadCurrenciesData()
    {
        var currencyResult = await _treasuryReportingRatesService.GetCurrenciesAsync();

        if(currencyResult == null)
        {
            _notificationService.Notify(NotificationSeverity.Error, "There was an error loading currencies.");
            return;
        }

        if (currencyResult!.IsSuccess == false)
        {
            _notificationService.Notify(NotificationSeverity.Error, currencyResult.ErrorMessage);
            return;
        }

        CurrenciesData = currencyResult.Object ?? new();
    }

    private async Task ShowGetUsernameModal()
    {
        if (IsLoggedIn)
            return;

        UserModel result = await _dialogService.OpenAsync<UsernameInputComponent>(string.Empty,
        options: new DialogOptions()
        {
            ShowTitle = false
        });

        if (result != null)
        {
            IsLoggedIn = true;
            Username = result.Username;
            _userStateService.Username = result.Username;
            _userStateService.IsLoggedIn = true;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async void PayNowButton_OnClick()
    {
        PayNowModel result = await _dialogService.OpenAsync<PayNowComponent>("Make a Payment", 
            options: new DialogOptions() 
            { 
               ShowTitle = false
            });

        if (result != null)
        {
            ulong cents = (ulong)Math.Round(result.Amount * 100m, MidpointRounding.AwayFromZero);

            var newModel = new TransactionModel
            {
                AmountTotalCents = cents,
                Description = result.Description
            };

            var addResult = await _transactionService.AddAsync(newModel);

            await InvokeAsync(StateHasChanged);
        }
    }

    private async void OnTransactionAdded(TransactionModel model)
    {
        TransactionsTotalCents = TransactionsTotalCents + model.AmountTotalCents;
        await InvokeAsync(StateHasChanged);
    }

    private async void OnTransactionDeleted(TransactionModel model)
    {
        TransactionsTotalCents = TransactionsTotalCents + model.AmountTotalCents;
        await InvokeAsync(StateHasChanged);
    }

    private void OnUserStateChanged()
    {
        Username = _userStateService.Username;
        IsLoggedIn = _userStateService.IsLoggedIn;
        StateHasChanged();
    }

    public void Dispose()
    {
        _userStateService.OnChange -= OnUserStateChanged;
        _applicationEventsService.TransactionAdded -= OnTransactionAdded;
        _applicationEventsService.TransactionDeleted -= OnTransactionDeleted;
    }
}