using Microsoft.EntityFrameworkCore;
using MiniBank.Api.Data;
using MiniBank.Api.Exceptions;
using MiniBank.Api.Models.DTOs.Transfer;
using MiniBank.Api.Models.Entities;
using MiniBank.Api.Services.Implementations;
using Xunit;

namespace MiniBank.Tests;

public class TransferServiceTests
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
    public async Task TransferAsync_ValidRequest_TransfersMoneyAndCreatesBothTransactions()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var customer = new Customer { FirstName = "Alice", LastName = "Wonderland", Email = "alice@example.com" };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var source = new Account { AccountNumber = "ACC-001", CustomerId = customer.Id, Balance = 500m };
        var dest = new Account { AccountNumber = "ACC-002", CustomerId = customer.Id, Balance = 100m };
        context.Accounts.AddRange(source, dest);
        await context.SaveChangesAsync();

        var service = new TransferService(context);

        var request = new TransferRequest
        {
            SourceAccountId = source.Id,
            DestinationAccountId = dest.Id,
            Amount = 200m
        };

        // Act
        var result = await service.TransferAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(300m, result.SourceNewBalance);

        var updatedSource = await context.Accounts.FindAsync(source.Id);
        var updatedDest = await context.Accounts.FindAsync(dest.Id);

        Assert.Equal(300m, updatedSource!.Balance);
        Assert.Equal(300m, updatedDest!.Balance);

        var sourceTx = await context.Transactions.FirstOrDefaultAsync(t => t.AccountId == source.Id && t.Type == TransactionType.TransferOut);
        var destTx = await context.Transactions.FirstOrDefaultAsync(t => t.AccountId == dest.Id && t.Type == TransactionType.TransferIn);

        Assert.NotNull(sourceTx);
        Assert.Equal(200m, sourceTx.Amount);

        Assert.NotNull(destTx);
        Assert.Equal(200m, destTx.Amount);
    }

    [Fact]
    public async Task TransferAsync_InsufficientFunds_ThrowsInsufficientFundsException()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var customer = new Customer { FirstName = "Bob", LastName = "Builder", Email = "bob@example.com" };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var source = new Account { AccountNumber = "ACC-003", CustomerId = customer.Id, Balance = 50m };
        var dest = new Account { AccountNumber = "ACC-004", CustomerId = customer.Id, Balance = 100m };
        context.Accounts.AddRange(source, dest);
        await context.SaveChangesAsync();

        var service = new TransferService(context);

        var request = new TransferRequest
        {
            SourceAccountId = source.Id,
            DestinationAccountId = dest.Id,
            Amount = 200m
        };

        // Act & Assert
        await Assert.ThrowsAsync<InsufficientFundsException>(() => service.TransferAsync(request));
    }

    [Fact]
    public async Task TransferAsync_SameSourceAndDestination_ThrowsBusinessRuleException()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var service = new TransferService(context);

        var request = new TransferRequest
        {
            SourceAccountId = 1,
            DestinationAccountId = 1,
            Amount = 50m
        };

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.TransferAsync(request));
    }
}
