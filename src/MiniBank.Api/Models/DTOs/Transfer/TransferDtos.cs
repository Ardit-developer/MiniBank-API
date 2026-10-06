using System.ComponentModel.DataAnnotations;

namespace MiniBank.Api.Models.DTOs.Transfer;

public class TransferRequest
{
    [Required(ErrorMessage = "SourceAccountId is required")]
    public int SourceAccountId { get; set; }

    [Required(ErrorMessage = "DestinationAccountId is required")]
    public int DestinationAccountId { get; set; }

    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Transfer amount must be greater than 0")]
    public decimal Amount { get; set; }
}

public class TransferResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int SourceAccountId { get; set; }
    public int DestinationAccountId { get; set; }
    public decimal TransferredAmount { get; set; }
    public decimal SourceNewBalance { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
