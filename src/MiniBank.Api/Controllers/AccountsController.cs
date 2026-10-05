using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniBank.Api.Data;
using MiniBank.Api.DTOs;
using MiniBank.Api.Models;

namespace MiniBank.Api.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController(MiniBankDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<AccountResponse>> Create(CreateAccountRequest request)
    {
        if (request.CustomerId <= 0 || string.IsNullOrWhiteSpace(request.AccountNumber))
            return BadRequest("CustomerId and AccountNumber are required.");

        if (!await db.Customers.AnyAsync(x => x.Id == request.CustomerId))
            return NotFound("Customer not found.");

        var accountNumber = request.AccountNumber.Trim();

        if (await db.Accounts.AnyAsync(x => x.AccountNumber == accountNumber))
            return BadRequest("An account with this account number already exists.");

        var account = new Account
        {
            CustomerId = request.CustomerId,
            AccountNumber = accountNumber,
            Balance = 0m
        };

        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = account.Id }, ToResponse(account));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AccountResponse>> GetById(int id)
    {
        var account = await db.Accounts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);

        if (account is null)
            return NotFound("Account not found.");

        return Ok(ToResponse(account));
    }

    [HttpPost("{id:int}/deposit")]
    public async Task<ActionResult<AccountResponse>> Deposit(int id, MoneyRequest request)
    {
        if (request.Amount <= 0)
            return BadRequest("Amount must be greater than 0.");

        var account = await db.Accounts.SingleOrDefaultAsync(x => x.Id == id);

        if (account is null)
            return NotFound("Account not found.");

        account.Balance += request.Amount;

        db.Transactions.Add(new BankTransaction
        {
            AccountId = account.Id,
            Type = TransactionType.Deposit,
            Amount = request.Amount
        });

        await db.SaveChangesAsync();
        return Ok(ToResponse(account));
    }

    [HttpPost("{id:int}/withdraw")]
    public async Task<ActionResult<AccountResponse>> Withdraw(int id, MoneyRequest request)
    {
        if (request.Amount <= 0)
            return BadRequest("Amount must be greater than 0.");

        var account = await db.Accounts.SingleOrDefaultAsync(x => x.Id == id);

        if (account is null)
            return NotFound("Account not found.");

        if (account.Balance < request.Amount)
            return BadRequest("Insufficient funds.");

        account.Balance -= request.Amount;

        db.Transactions.Add(new BankTransaction
        {
            AccountId = account.Id,
            Type = TransactionType.Withdraw,
            Amount = request.Amount
        });

        await db.SaveChangesAsync();
        return Ok(ToResponse(account));
    }

    [HttpGet("{id:int}/transactions")]
    public async Task<ActionResult<IEnumerable<TransactionResponse>>> GetTransactions(int id)
    {
        if (!await db.Accounts.AnyAsync(x => x.Id == id))
            return NotFound("Account not found.");

        var transactions = await db.Transactions
            .AsNoTracking()
            .Where(x => x.AccountId == id)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Select(x => new TransactionResponse(
                x.Id, x.AccountId, x.Type.ToString(), x.Amount, x.CreatedAt))
            .ToListAsync();

        return Ok(transactions);
    }

    private static AccountResponse ToResponse(Account account) =>
        new(account.Id, account.AccountNumber, account.CustomerId, account.Balance, account.CreatedAt);
}
