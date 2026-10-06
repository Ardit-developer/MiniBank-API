namespace MiniBank.Api.DTOs;

public record CreateCustomerRequest(string FirstName, string LastName, string Email);

public record CustomerResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    DateTime CreatedAt,
    IReadOnlyList<AccountSummaryResponse> Accounts);

public record AccountSummaryResponse(
    int Id,
    string AccountNumber,
    decimal Balance);
