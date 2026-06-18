using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTransferCenter.Application.Dtos.Transaction.Deposit;
using MoneyTransferCenter.Application.Dtos.Transaction.History;
using MoneyTransferCenter.Application.Dtos.Transaction.Transfer;
using MoneyTransferCenter.Application.Dtos.Transaction.WithDraw;
using MoneyTransferCenter.Application.Interfaces;
using System.Security.Claims;

namespace MoneyTransferCenter.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
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

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] TransactionHistoryRequestDto request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _transactionService.GetHistoryAsync(userId, request);
        return Ok(result);
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawRequestDto request)
    {
        Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        WithdrawResponseDto response = await _transactionService.WithdrawAsync(userId, request);
        return Ok(response);
    }
}
