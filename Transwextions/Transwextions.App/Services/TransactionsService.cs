using Microsoft.EntityFrameworkCore;
using Transwextions.App.Services.Interfaces;
using Transwextions.Data;
using Transwextions.Data.Models;

namespace Transwextions.App.Services;

public class TransactionsService : ITransactionService
{
    protected readonly TranswextionsContext _context;

    public TransactionsService(TranswextionsContext context)
    {
        _context = context;
    }

    public async Task<ServiceResult<List<TransactionModel>>> GetAllAsync()
    {
        var result = await _context.Transactions
            .AsNoTracking()
            .Where(p => p.IsDeleted == false)
            .ToListAsync();

        if (result == null)
            return ServiceResult<List<TransactionModel>>.Failure("Database returned a null value for Transactions.");

        return ServiceResult<List<TransactionModel>>.Success(result);
    }

    public async Task<ServiceResult<object>> AddAsync(TransactionModel model, DateTime? transactionDateOverride = null)
    {
        try
        {
            if (model == null)
                return ServiceResult<object>.Failure("Model is null.");

            if (string.IsNullOrWhiteSpace(model.Description))
                return ServiceResult<object>.Failure("Model description is null.");

            if (model.Description.Length > 50)
                return ServiceResult<object>.Failure("Model description character limit is 50.");

            if (model.AmountTotalCents < 0)
                return ServiceResult<object>.Failure("Model AmountTotalCents is a negative value.");

            model.UniqueIdentifier = Guid.NewGuid();
            model.TransactionDateUtc = transactionDateOverride ?? DateTime.UtcNow;

            await _context.Transactions.AddAsync(model);
            await _context.SaveChangesAsync();

            return ServiceResult<object>.Success(new());
        }
        catch (Exception ex)
        {
            return ServiceResult<object>.Failure(ex.Message);
        }
    }

    public async Task<ServiceResult<object>> DeleteAsync(Guid transactionGuid)
    {
        var modelResult = await GetByGuidAsync(transactionGuid);

        if (modelResult.IsSuccess == false || modelResult.Object == null)
            return ServiceResult<object>.Failure("Model does not exist:" + transactionGuid.ToString());

        _context.Transactions.Remove(modelResult.Object);
        await _context.SaveChangesAsync();

        return ServiceResult<object>.Success(new());
    }

    public async Task<ServiceResult<TransactionModel?>> GetByGuidAsync(Guid transactionGuid)
    {
        var result = await _context.Transactions
            .AsNoTracking()
            .Where(p => p.UniqueIdentifier == transactionGuid)
            .FirstOrDefaultAsync();

        if (result == null)
            return ServiceResult<TransactionModel?>.Failure("Model does not exist:" + transactionGuid.ToString());

        return ServiceResult<TransactionModel?>.Success(result);
    }
}