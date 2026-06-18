using MoneyTransferCenter.Application.Dtos.Transaction.Deposit;
using MoneyTransferCenter.Application.Dtos.Transaction.History;
using MoneyTransferCenter.Application.Dtos.Transaction.Transfer;
using MoneyTransferCenter.Application.Dtos.Transaction.WithDraw;

namespace MoneyTransferCenter.Application.Interfaces;

public interface ITransactionService
{
    Task<DepositResponseDto> DepositAsync(Guid userId, DepositRequestDto request);
    Task<TransferResponseDto> TransferAsync(Guid userId, TransferRequestDto request);
    Task<List<TransactionHistoryItemResponseDto>> GetHistoryAsync(Guid userId, TransactionHistoryRequestDto request);
    Task<WithdrawResponseDto> WithdrawAsync(Guid userId, WithdrawRequestDto request);


}
