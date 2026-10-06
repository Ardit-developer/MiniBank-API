using Microsoft.AspNetCore.Mvc;
using MiniBank.Api.Models.DTOs.Account;
using MiniBank.Api.Models.DTOs.Common;
using MiniBank.Api.Services.Interfaces;

namespace MiniBank.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    /// <summary>
    /// Opens a new account for an existing customer.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountResponse>> CreateAccount([FromBody] CreateAccountRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _accountService.CreateAccountAsync(request);
        return CreatedAtAction(nameof(GetAccountById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Returns account details including the current balance.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AccountDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountDetailResponse>> GetAccountById(int id)
    {
        var account = await _accountService.GetAccountByIdAsync(id);
        return Ok(account);
    }

    /// <summary>
    /// Deposits money into an account.
    /// </summary>
    [HttpPost("{id:int}/deposit")]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountResponse>> Deposit(int id, [FromBody] DepositRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _accountService.DepositAsync(id, request.Amount);
        return Ok(result);
    }

    /// <summary>
    /// Withdraws money from an account.
    /// </summary>
    [HttpPost("{id:int}/withdraw")]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountResponse>> Withdraw(int id, [FromBody] WithdrawRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _accountService.WithdrawAsync(id, request.Amount);
        return Ok(result);
    }

    /// <summary>
    /// Lists the transactions of an account (newest first) with optional pagination.
    /// </summary>
    [HttpGet("{id:int}/transactions")]
    [ProducesResponseType(typeof(PagedResult<TransactionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<TransactionResponse>>> GetTransactions(
        int id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _accountService.GetAccountTransactionsAsync(id, pageNumber, pageSize);
        return Ok(result);
    }
}
