using Microsoft.AspNetCore.Components;
using Transwextions.Data.Models;

namespace Transwextions.App.Components.Modals;

public partial class ViewTransactionComponent
{
    [Parameter]
    public TransactionModel? Transaction { get; set; }
}