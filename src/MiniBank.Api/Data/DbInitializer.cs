using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using MiniBank.Api.Models.Entities;

namespace MiniBank.Api.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(MiniBankDbContext context)
    {
        try
        {
            var databaseCreator = context.Database.GetService<IDatabaseCreator>() as IRelationalDatabaseCreator;
            if (databaseCreator != null)
            {
                if (!await databaseCreator.ExistsAsync())
                {
                    await databaseCreator.CreateAsync();
                }

                if (!await databaseCreator.HasTablesAsync())
                {
                    await databaseCreator.CreateTablesAsync();
                }
            }
        }
        catch
        {
            try
            {
                await context.Database.MigrateAsync();
            }
            catch
            {
                await context.Database.EnsureCreatedAsync();
            }
        }

        var hasData = await context.Customers.AnyAsync();
        if (!hasData)
        {
            var customer1 = new Customer
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            };

            var customer2 = new Customer
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            };

            context.Customers.AddRange(customer1, customer2);
            await context.SaveChangesAsync();

            var account1 = new Account
            {
                AccountNumber = "ACC-100001",
                CustomerId = customer1.Id,
                Balance = 1500.00m,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            };

            var account2 = new Account
            {
                AccountNumber = "ACC-100002",
                CustomerId = customer2.Id,
                Balance = 800.00m,
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            };

            context.Accounts.AddRange(account1, account2);
            await context.SaveChangesAsync();

            var tx1 = new Transaction
            {
                AccountId = account1.Id,
                Type = TransactionType.Deposit,
                Amount = 1500.00m,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            };

            var tx2 = new Transaction
            {
                AccountId = account2.Id,
                Type = TransactionType.Deposit,
                Amount = 800.00m,
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            };

            context.Transactions.AddRange(tx1, tx2);
            await context.SaveChangesAsync();
        }
    }
}
