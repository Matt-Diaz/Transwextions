using Radzen;
using Transwextions.App.Services.Interfaces;
using Transwextions.Data.Models;

namespace Transwextions.App.Components;

public partial class TransactionsDataTableComponent : IDisposable
{
    private readonly ITransactionService _transactionService;
    private readonly IApplicationEventsService _applicationEventsService;

    public TransactionsDataTableComponent(ITransactionService transactionService, IApplicationEventsService applicationEventsService)
    {
        _transactionService = transactionService;
        _applicationEventsService = applicationEventsService;

        _applicationEventsService.TransactionAdded += OnTransactionAdded;
        _applicationEventsService.TransactionDeleted += OnTransactionDeleted;
    }

    private bool IsLoading;
    IEnumerable<TransactionModel> TransactionsData = Array.Empty<TransactionModel>();

    private async void OnTransactionAdded(TransactionModel model)
    {
        TransactionsData = TransactionsData.Append(model);
        await InvokeAsync(StateHasChanged);
    }

    private async void OnTransactionDeleted(TransactionModel model)
    {
        TransactionsData = TransactionsData.Where(p => p.UniqueIdentifier != model.UniqueIdentifier);
        await InvokeAsync(StateHasChanged);
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    async Task LoadData()
    {
        IsLoading = true;

        var result = await _transactionService.GetAllAsync();
        if (!result.IsSuccess || result.Object is null)
        {
            _notificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Error",
                Detail = "Failed to load transactions:" + result.ErrorMessage,
                Duration = 4000
            });
            IsLoading = false;
            return;
        }

        TransactionsData = result.Object;

        IsLoading = false;
    }

    void View(TransactionModel t)
    {
        _notificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Info, Summary = "View", Detail = $"{t.Id}" });
    }

    async Task Delete(TransactionModel t)
    {
        _notificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Success, Summary = "Deleted", Detail = $"{t.Id}" });
    }

    public void Dispose()
    {
        _applicationEventsService.TransactionAdded -= OnTransactionAdded;
        _applicationEventsService.TransactionDeleted -= OnTransactionDeleted;
    }
}