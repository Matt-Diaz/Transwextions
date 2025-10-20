using Transwextions.App.Services.Interfaces;
using Transwextions.Data.Models;

namespace Transwextions.Tests.Fakes;

public sealed class FakeApplicationEventsService : IApplicationEventsService
{
    public readonly List<TransactionModel> Added = new();
    public readonly List<TransactionModel> Deleted = new();

    public event Action<TransactionModel>? TransactionAdded;
    public event Action<TransactionModel>? TransactionDeleted;

    public void NotifyTransactionAdded(TransactionModel model) => Added.Add(model);
    public void NotifyTransactionDeleted(TransactionModel model) => Deleted.Add(model);
}