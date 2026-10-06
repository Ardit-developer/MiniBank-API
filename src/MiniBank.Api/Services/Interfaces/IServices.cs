using MiniBank.Api.Models.DTOs.Common;
using MiniBank.Api.Models.DTOs.Customer;
using MiniBank.Api.Models.DTOs.Account;
using MiniBank.Api.Models.DTOs.Transfer;

namespace MiniBank.Api.Services.Interfaces;

public interface ICustomerService
{
    Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request);
    Task<IEnumerable<CustomerResponse>> GetAllCustomersAsync();
    Task<CustomerWithAccountsResponse> GetCustomerByIdAsync(int id);
}

public interface IAccountService
{
    Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request);
    Task<AccountDetailResponse> GetAccountByIdAsync(int id);
    Task<AccountResponse> DepositAsync(int accountId, decimal amount);
    Task<AccountResponse> WithdrawAsync(int accountId, decimal amount);
    Task<PagedResult<TransactionResponse>> GetAccountTransactionsAsync(int accountId, int pageNumber = 1, int pageSize = 10);
}

public interface ITransferService
{
    Task<TransferResponse> TransferAsync(TransferRequest request);
}
