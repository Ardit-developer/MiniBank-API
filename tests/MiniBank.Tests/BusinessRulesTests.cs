using Microsoft.EntityFrameworkCore;
using MiniBank.Api.Data;
using MiniBank.Api.Exceptions;
using MiniBank.Api.Models.DTOs.Transfer;
using MiniBank.Api.Models.Entities;
using MiniBank.Api.Services.Implementations;
using Xunit;

namespace MiniBank.Tests;

public class BusinessRulesTests
{
    private MiniBankDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<MiniBankDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new MiniBankDbContext(options);
    }

    [Fact]
    public async Task AmountMustBeGreaterThanZero_Deposit()
    {
        using var context = CreateInMemoryDbContext();
        var service = new AccountService(context);

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.DepositAsync(1, 0m));
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.DepositAsync(1, -50m));
    }

    [Fact]
    public async Task AmountMustBeGreaterThanZero_Withdraw()
    {
        using var context = CreateInMemoryDbContext();
        var service = new AccountService(context);

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.WithdrawAsync(1, 0m));
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.WithdrawAsync(1, -50m));
    }

    [Fact]
    public async Task WithdrawalsFail_IfNotEnoughMoney()
    {
        using var context = CreateInMemoryDbContext();
        var customer = new Customer { FirstName = "Test", LastName = "User", Email = "test@example.com" };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var account = new Account { AccountNumber = "ACC-001", CustomerId = customer.Id, Balance = 50m };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var service = new AccountService(context);

        await Assert.ThrowsAsync<InsufficientFundsException>(() => service.WithdrawAsync(account.Id, 100m));
    }

    [Fact]
    public async Task TransfersFail_IfNotEnoughMoney()
    {
        using var context = CreateInMemoryDbContext();
        var customer = new Customer { FirstName = "Test", LastName = "User", Email = "test2@example.com" };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var source = new Account { AccountNumber = "ACC-101", CustomerId = customer.Id, Balance = 20m };
        var dest = new Account { AccountNumber = "ACC-102", CustomerId = customer.Id, Balance = 100m };
        context.Accounts.AddRange(source, dest);
        await context.SaveChangesAsync();

        var service = new TransferService(context);
        var request = new TransferRequest { SourceAccountId = source.Id, DestinationAccountId = dest.Id, Amount = 50m };

        await Assert.ThrowsAsync<InsufficientFundsException>(() => service.TransferAsync(request));
    }
}
