namespace MiniBank.Api.DTOs;

public record CreateAccountRequest(int CustomerId, string AccountNumber);

public record AccountResponse(
    int Id,
    string AccountNumber,
    int CustomerId,
    decimal Balance,
    DateTime CreatedAt);

public record MoneyRequest(decimal Amount);

public record TransferRequest(int FromAccountId, int ToAccountId, decimal Amount);

public record TransactionResponse(
    int Id,
    int AccountId,
    string Type,
    decimal Amount,
    DateTime CreatedAt);
