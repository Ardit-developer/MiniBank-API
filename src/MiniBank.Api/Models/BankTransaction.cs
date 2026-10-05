namespace MiniBank.Api.Models;

public class BankTransaction
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }

    public Account Account { get; set; } = null!;
}

public enum TransactionType
{
    Deposit,
    Withdraw,
    TransferIn,
    TransferOut
}
