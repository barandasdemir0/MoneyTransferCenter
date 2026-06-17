using Microsoft.AspNetCore.Mvc;
using MoneyTransferCenter.Application.Dtos.Transaction;
using MoneyTransferCenter.Application.Interfaces;
using System.Security.Claims;

namespace MoneyTransferCenter.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TransactionController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] DepositRequestDto request)
    {
        // JWT'den kullanıcı ID'sini al
        Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        DepositResponseDto response = await _transactionService.DepositAsync(userId, request);
        return Ok(response);
    }


    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequestDto request)
    {
        Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        TransferResponseDto response = await _transactionService.TransferAsync(userId, request);
        return Ok(response);
    }
}
