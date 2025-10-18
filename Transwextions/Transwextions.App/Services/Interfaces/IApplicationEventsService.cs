using Transwextions.Data.Models;

namespace Transwextions.App.Services.Interfaces;

public interface IApplicationEventsService
{
    public event Action<TransactionModel>? TransactionAdded;

    public event Action<TransactionModel>? TransactionDeleted;

    public void NotifyTransactionAdded(TransactionModel model);
    public void NotifyTransactionDeleted(TransactionModel model);
}