using Microsoft.EntityFrameworkCore;
using MiniBank.Api.Data;
using MiniBank.Api.Exceptions;
using MiniBank.Api.Models.DTOs.Account;
using MiniBank.Api.Models.DTOs.Common;
using MiniBank.Api.Models.Entities;
using MiniBank.Api.Services.Interfaces;

namespace MiniBank.Api.Services.Implementations;

public class AccountService : IAccountService
{
    private readonly MiniBankDbContext _context;

    public AccountService(MiniBankDbContext context)
    {
        _context = context;
    }

    public async Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request)
    {
        var customerExists = await _context.Customers.AnyAsync(c => c.Id == request.CustomerId);
        if (!customerExists)
        {
            throw new NotFoundException($"Customer with ID {request.CustomerId} was not found.");
        }

        var accountNumber = GenerateAccountNumber();

        // Ensure unique account number
        while (await _context.Accounts.AnyAsync(a => a.AccountNumber == accountNumber))
        {
            accountNumber = GenerateAccountNumber();
        }

        var account = new Account
        {
            AccountNumber = accountNumber,
            CustomerId = request.CustomerId,
            Balance = 0.00m,
            CreatedAt = DateTime.UtcNow
        };

        if (request.InitialDeposit > 0)
        {
            account.Balance = request.InitialDeposit;
            account.Transactions.Add(new Transaction
            {
                Type = TransactionType.Deposit,
                Amount = request.InitialDeposit,
                CreatedAt = DateTime.UtcNow
            });
        }

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        return new AccountResponse
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber,
            CustomerId = account.CustomerId,
            Balance = account.Balance,
            CreatedAt = account.CreatedAt
        };
    }

    public async Task<AccountDetailResponse> GetAccountByIdAsync(int id)
    {
        var account = await _context.Accounts
            .AsNoTracking()
            .Include(a => a.Customer)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (account == null)
        {
            throw new NotFoundException($"Account with ID {id} was not found.");
        }

        return new AccountDetailResponse
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber,
            CustomerId = account.CustomerId,
            CustomerFullName = $"{account.Customer.FirstName} {account.Customer.LastName}",
            CustomerEmail = account.Customer.Email,
            Balance = account.Balance,
            CreatedAt = account.CreatedAt
        };
    }

    public async Task<AccountResponse> DepositAsync(int accountId, decimal amount)
    {
        if (amount <= 0)
        {
            throw new BusinessRuleException("Deposit amount must be greater than 0.");
        }

        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);
        if (account == null)
        {
            throw new NotFoundException($"Account with ID {accountId} was not found.");
        }

        account.Balance += amount;

        var transaction = new Transaction
        {
            AccountId = account.Id,
            Type = TransactionType.Deposit,
            Amount = amount,
            CreatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        return new AccountResponse
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber,
            CustomerId = account.CustomerId,
            Balance = account.Balance,
            CreatedAt = account.CreatedAt
        };
    }

    public async Task<AccountResponse> WithdrawAsync(int accountId, decimal amount)
    {
        if (amount <= 0)
        {
            throw new BusinessRuleException("Withdrawal amount must be greater than 0.");
        }

        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);
        if (account == null)
        {
            throw new NotFoundException($"Account with ID {accountId} was not found.");
        }

        if (account.Balance < amount)
        {
            throw new InsufficientFundsException($"Insufficient funds. Current balance is {account.Balance:C2}, but attempted to withdraw {amount:C2}.");
        }

        account.Balance -= amount;

        var transaction = new Transaction
        {
            AccountId = account.Id,
            Type = TransactionType.Withdraw,
            Amount = amount,
            CreatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        return new AccountResponse
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber,
            CustomerId = account.CustomerId,
            Balance = account.Balance,
            CreatedAt = account.CreatedAt
        };
    }

    public async Task<PagedResult<TransactionResponse>> GetAccountTransactionsAsync(int accountId, int pageNumber = 1, int pageSize = 10)
    {
        var accountExists = await _context.Accounts.AnyAsync(a => a.Id == accountId);
        if (!accountExists)
        {
            throw new NotFoundException($"Account with ID {accountId} was not found.");
        }

        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var query = _context.Transactions
            .AsNoTracking()
            .Where(t => t.AccountId == accountId);

        var totalCount = await query.CountAsync();

        var transactions = await query
            .OrderByDescending(t => t.CreatedAt)
            .ThenByDescending(t => t.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TransactionResponse
            {
                Id = t.Id,
                AccountId = t.AccountId,
                Type = t.Type.ToString(),
                Amount = t.Amount,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<TransactionResponse>
        {
            Items = transactions,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    private static string GenerateAccountNumber()
    {
        var random = new Random();
        return $"ACC-{random.Next(100000, 999999)}";
    }
}
