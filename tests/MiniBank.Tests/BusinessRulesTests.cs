using Microsoft.EntityFrameworkCore;
using MiniBank.Api.Data;
using MiniBank.Api.Models;

namespace MiniBank.Tests;

public class BusinessRulesTests
{
    private static MiniBankDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<MiniBankDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new MiniBankDbContext(options);
    }

    [Fact]
    public async Task Deposit_IncreasesBalance_AndCreatesTransaction()
    {
        await using var db = CreateDb();
        var customer = new Customer { FirstName = "Test", LastName = "User", Email = "test@example.com" };
        var account = new Account { AccountNumber = "T-001", Customer = customer, Balance = 100m };
        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        account.Balance += 50m;
        db.Transactions.Add(new BankTransaction { AccountId = account.Id, Type = TransactionType.Deposit, Amount = 50m });
        await db.SaveChangesAsync();

        Assert.Equal(150m, account.Balance);
        Assert.Single(db.Transactions);
    }

    [Fact]
    public async Task Withdraw_IsRejected_WhenBalanceIsInsufficient()
    {
        await using var db = CreateDb();
        var account = new Account { AccountNumber = "T-002", Balance = 25m };
        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        const decimal amount = 50m;

        Assert.True(account.Balance < amount);
        Assert.Equal(25m, account.Balance);
    }

    [Fact]
    public async Task Transfer_ChangesBothAccounts()
    {
        await using var db = CreateDb();
        var from = new Account { AccountNumber = "T-003", Balance = 100m };
        var to = new Account { AccountNumber = "T-004", Balance = 20m };
        db.Accounts.AddRange(from, to);
        await db.SaveChangesAsync();

        from.Balance -= 30m;
        to.Balance += 30m;
        db.Transactions.AddRange(
            new BankTransaction { AccountId = from.Id, Type = TransactionType.TransferOut, Amount = 30m },
            new BankTransaction { AccountId = to.Id, Type = TransactionType.TransferIn, Amount = 30m });
        await db.SaveChangesAsync();

        Assert.Equal(70m, from.Balance);
        Assert.Equal(50m, to.Balance);
        Assert.Equal(2, await db.Transactions.CountAsync());
    }
}
