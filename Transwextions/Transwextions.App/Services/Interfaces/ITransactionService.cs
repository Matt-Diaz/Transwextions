using Transwextions.Data.Models;

namespace Transwextions.App.Services.Interfaces;

public interface ITransactionService
{
    public Task<ServiceResult<List<TransactionModel>>> GetAllAsync();
    public Task<ServiceResult<object>> AddAsync(TransactionModel model, DateTime? transactionDateOverride = null);
    public Task<ServiceResult<object>> DeleteAsync(Guid transactionGuid);
    public Task<ServiceResult<TransactionModel?>> GetByGuidAsync(Guid transactionGuid);
    public Task<ServiceResult<ulong?>> GetTransactionsTotalCentsAsync();
}