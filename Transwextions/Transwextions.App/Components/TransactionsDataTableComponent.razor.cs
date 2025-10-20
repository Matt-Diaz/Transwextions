using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Radzen;
using Transwextions.App.Components.Modals;
using Transwextions.App.Services.Interfaces;
using Transwextions.Data.Models;

namespace Transwextions.App.Components;

public partial class TransactionsDataTableComponent : IDisposable
{
    private readonly ITransactionService _transactionService;
    private readonly IApplicationEventsService _applicationEventsService;

    [Parameter]
    public List<string> CurrenciesData { get; set; } = new();

    public TransactionsDataTableComponent(ITransactionService transactionService, IApplicationEventsService applicationEventsService)
    {
        _transactionService = transactionService;
        _applicationEventsService = applicationEventsService;

        _applicationEventsService.TransactionAdded += OnTransactionAdded;
        _applicationEventsService.TransactionDeleted += OnTransactionDeleted;
    }

    private bool IsLoading;
    IEnumerable<TransactionModel> TransactionsData = Array.Empty<TransactionModel>();

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

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

    private async Task LoadData()
    {
        IsLoading = true;

        var result = await _transactionService.GetAllAsync();

        // Maybe Notify Error & Return
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

    private async void View(TransactionModel model)
    {
        var result = await _dialogService.OpenAsync<ViewTransactionComponent>(
            "View Transaction",
            parameters: new Dictionary<string, object?>
            {
                { "Transaction", model },
                { "CurrenciesData", CurrenciesData }
            },
            options: new DialogOptions()
            {
                ShowTitle = true,
                ShowClose = true
            });
    }

    private async Task Delete(TransactionModel model)
    {
        if(model != null && model.UniqueIdentifier != null)
        {
            var deleteResult = await _transactionService.DeleteAsync((Guid)model.UniqueIdentifier!);

            if (deleteResult != null)
            {
                if (deleteResult.IsSuccess)
                {
                    _notificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Success",
                        Detail = "Transaction deleted successfully.",
                        Duration = 4000
                    });

                    _applicationEventsService.NotifyTransactionDeleted(model);
                }
                else
                {
                    _notificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = "Failed to delete transaction: " + deleteResult.ErrorMessage,
                        Duration = 4000
                    });
                }
            }
        }
        else
        {
            _notificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Error",
                Detail = "Transaction model is null.",
                Duration = 4000
            });
        }
    }

    public void Dispose()
    {
        _applicationEventsService.TransactionAdded -= OnTransactionAdded;
        _applicationEventsService.TransactionDeleted -= OnTransactionDeleted;
    }
}