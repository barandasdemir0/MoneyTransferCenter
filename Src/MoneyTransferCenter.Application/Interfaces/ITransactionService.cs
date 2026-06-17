using MoneyTransferCenter.Application.Dtos.Transaction;

namespace MoneyTransferCenter.Application.Interfaces;

public interface ITransactionService
{
    Task<DepositResponseDto> DepositAsync(Guid userId, DepositRequestDto request);
}
