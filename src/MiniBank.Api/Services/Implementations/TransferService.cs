using Microsoft.EntityFrameworkCore;
using MiniBank.Api.Data;
using MiniBank.Api.Exceptions;
using MiniBank.Api.Models.DTOs.Transfer;
using MiniBank.Api.Models.Entities;
using MiniBank.Api.Services.Interfaces;

namespace MiniBank.Api.Services.Implementations;

public class TransferService : ITransferService
{
    private readonly MiniBankDbContext _context;

    public TransferService(MiniBankDbContext context)
    {
        _context = context;
    }

    public async Task<TransferResponse> TransferAsync(TransferRequest request)
    {
        if (request.Amount <= 0)
        {
            throw new BusinessRuleException("Transfer amount must be greater than 0.");
        }

        if (request.SourceAccountId == request.DestinationAccountId)
        {
            throw new BusinessRuleException("Source and destination accounts cannot be the same.");
        }

        // Use a database transaction to guarantee ACID atomicity
        using var dbTransaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var sourceAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == request.SourceAccountId);
            if (sourceAccount == null)
            {
                throw new NotFoundException($"Source account with ID {request.SourceAccountId} was not found.");
            }

            var destAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == request.DestinationAccountId);
            if (destAccount == null)
            {
                throw new NotFoundException($"Destination account with ID {request.DestinationAccountId} was not found.");
            }

            if (sourceAccount.Balance < request.Amount)
            {
                throw new InsufficientFundsException($"Insufficient funds in source account (Current balance: {sourceAccount.Balance:C2}, attempted: {request.Amount:C2}).");
            }

            // Update balances
            sourceAccount.Balance -= request.Amount;
            destAccount.Balance += request.Amount;

            var now = DateTime.UtcNow;

            // Create TransferOut transaction for source account
            var sourceTx = new Transaction
            {
                AccountId = sourceAccount.Id,
                Type = TransactionType.TransferOut,
                Amount = request.Amount,
                CreatedAt = now
            };

            // Create TransferIn transaction for destination account
            var destTx = new Transaction
            {
                AccountId = destAccount.Id,
                Type = TransactionType.TransferIn,
                Amount = request.Amount,
                CreatedAt = now
            };

            _context.Transactions.Add(sourceTx);
            _context.Transactions.Add(destTx);

            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return new TransferResponse
            {
                Success = true,
                Message = "Transfer completed successfully.",
                SourceAccountId = sourceAccount.Id,
                DestinationAccountId = destAccount.Id,
                TransferredAmount = request.Amount,
                SourceNewBalance = sourceAccount.Balance,
                Timestamp = now
            };
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }
}
