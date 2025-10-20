using Transwextions.App.Services.Interfaces;
using Transwextions.Data.Models;

namespace Transwextions.App.Services;

public class ApplicationEventsService : IApplicationEventsService
{
    public event Action<TransactionModel>? TransactionAdded;

    public event Action<TransactionModel>? TransactionDeleted;

    public void NotifyTransactionAdded(TransactionModel model) => TransactionAdded?.Invoke(model);
    public void NotifyTransactionDeleted(TransactionModel model) => TransactionDeleted?.Invoke(model);
}