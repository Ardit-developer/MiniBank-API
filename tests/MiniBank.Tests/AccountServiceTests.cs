using Microsoft.EntityFrameworkCore;
using MiniBank.Api.Data;
using MiniBank.Api.Exceptions;
using MiniBank.Api.Models.DTOs.Account;
using MiniBank.Api.Models.Entities;
using MiniBank.Api.Services.Implementations;
using Xunit;

namespace MiniBank.Tests;

public class AccountServiceTests
{
    private MiniBankDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<MiniBankDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new MiniBankDbContext(options);
    }

    [Fact]
    public async Task DepositAsync_ValidAmount_IncreasesBalanceAndCreatesTransaction()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var customer = new Customer { FirstName = "John", LastName = "Doe", Email = "john@example.com" };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var account = new Account { AccountNumber = "ACC-123456", CustomerId = customer.Id, Balance = 100m };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var service = new AccountService(context);

        // Act
        var result = await service.DepositAsync(account.Id, 50m);

        // Assert
        Assert.Equal(150m, result.Balance);

        var savedAccount = await context.Accounts.FindAsync(account.Id);
        Assert.NotNull(savedAccount);
        Assert.Equal(150m, savedAccount.Balance);

        var tx = await context.Transactions.FirstOrDefaultAsync(t => t.AccountId == account.Id);
        Assert.NotNull(tx);
        Assert.Equal(TransactionType.Deposit, tx.Type);
        Assert.Equal(50m, tx.Amount);
    }

    [Fact]
    public async Task DepositAsync_NegativeOrZeroAmount_ThrowsBusinessRuleException()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var service = new AccountService(context);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.DepositAsync(1, 0m));
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.DepositAsync(1, -20m));
    }

    [Fact]
    public async Task WithdrawAsync_ValidAmount_DecreasesBalanceAndCreatesTransaction()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var customer = new Customer { FirstName = "Jane", LastName = "Doe", Email = "jane@example.com" };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var account = new Account { AccountNumber = "ACC-654321", CustomerId = customer.Id, Balance = 200m };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var service = new AccountService(context);

        // Act
        var result = await service.WithdrawAsync(account.Id, 75m);

        // Assert
        Assert.Equal(125m, result.Balance);

        var tx = await context.Transactions.FirstOrDefaultAsync(t => t.AccountId == account.Id);
        Assert.NotNull(tx);
        Assert.Equal(TransactionType.Withdraw, tx.Type);
        Assert.Equal(75m, tx.Amount);
    }

    [Fact]
    public async Task WithdrawAsync_InsufficientBalance_ThrowsInsufficientFundsException()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var customer = new Customer { FirstName = "Jane", LastName = "Doe", Email = "jane2@example.com" };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var account = new Account { AccountNumber = "ACC-999999", CustomerId = customer.Id, Balance = 50m };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var service = new AccountService(context);

        // Act & Assert
        await Assert.ThrowsAsync<InsufficientFundsException>(() => service.WithdrawAsync(account.Id, 100m));
    }
}
