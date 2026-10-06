using System.ComponentModel.DataAnnotations;

namespace MiniBank.Api.Models.DTOs.Account;

public class CreateAccountRequest
{
    [Required(ErrorMessage = "CustomerId is required")]
    public int CustomerId { get; set; }

    [Range(0.00, double.MaxValue, ErrorMessage = "Initial deposit cannot be negative")]
    public decimal InitialDeposit { get; set; } = 0.00m;
}

public class AccountResponse
{
    public int Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AccountDetailResponse
{
    public int Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerFullName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DepositRequest
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
}

public class WithdrawRequest
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
}

public class TransactionResponse
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}
