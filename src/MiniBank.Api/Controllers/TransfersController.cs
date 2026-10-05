using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniBank.Api.Data;
using MiniBank.Api.DTOs;
using MiniBank.Api.Models;

namespace MiniBank.Api.Controllers;

[ApiController]
[Route("api/transfers")]
public class TransfersController(MiniBankDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Transfer(TransferRequest request)
    {
        if (request.Amount <= 0)
            return BadRequest("Amount must be greater than 0.");

        if (request.FromAccountId == request.ToAccountId)
            return BadRequest("Source and destination accounts must be different.");

        await using var transaction = await db.Database.BeginTransactionAsync();

        var accountIds = new[] { request.FromAccountId, request.ToAccountId };

        var accounts = await db.Accounts
            .Where(x => accountIds.Contains(x.Id))
            .ToListAsync();

        var from = accounts.SingleOrDefault(x => x.Id == request.FromAccountId);
        var to = accounts.SingleOrDefault(x => x.Id == request.ToAccountId);

        if (from is null || to is null)
        {
            await transaction.RollbackAsync();
            return NotFound("One or both accounts were not found.");
        }

        if (from.Balance < request.Amount)
        {
            await transaction.RollbackAsync();
            return BadRequest("Insufficient funds.");
        }

        from.Balance -= request.Amount;
        to.Balance += request.Amount;

        db.Transactions.AddRange(
            new BankTransaction
            {
                AccountId = from.Id,
                Type = TransactionType.TransferOut,
                Amount = request.Amount
            },
            new BankTransaction
            {
                AccountId = to.Id,
                Type = TransactionType.TransferIn,
                Amount = request.Amount
            });

        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new
        {
            message = "Transfer completed successfully.",
            fromAccountId = from.Id,
            toAccountId = to.Id,
            amount = request.Amount,
            fromBalance = from.Balance,
            toBalance = to.Balance
        });
    }
}
