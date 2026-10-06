using MiniBank.Api.Models;

namespace MiniBank.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(MiniBankDbContext db)
    {

            return;

        Customer customer = new Customer
        {
            FirstName = "Demo",
            LastName = "Customer",
            Email = "demo@minibank.local"
        };

        var account = new Account
        {
            AccountNumber = "10000001",
            Customer = customer,
            Balance = 1000m
        };

        db.Customers.Add(customer);
        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        db.Transactions.Add(new BankTransaction
        {
            AccountId = account.Id,
            Type = TransactionType.Deposit,
            Amount = 1000m
        });

        await db.SaveChangesAsync();
    }
}
