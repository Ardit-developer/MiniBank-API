namespace MiniBank.Api.Models.Entities;

public class Transaction
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Account Account { get; set; } = null!;
}
