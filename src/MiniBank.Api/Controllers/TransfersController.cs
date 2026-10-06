using Microsoft.AspNetCore.Mvc;
using MiniBank.Api.Models.DTOs.Transfer;
using MiniBank.Api.Services.Interfaces;

namespace MiniBank.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransfersController : ControllerBase
{
    private readonly ITransferService _transferService;

    public TransfersController(ITransferService transferService)
    {
        _transferService = transferService;
    }

    /// <summary>
    /// Transfers money atomically between two bank accounts.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TransferResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransferResponse>> Transfer([FromBody] TransferRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _transferService.TransferAsync(request);
        return Ok(result);
    }
}
