using Mapster;
using MoneyTransferCenter.Application.Dtos.Transaction;

namespace MoneyTransferCenter.Application.Dtos.Mappings;

public sealed class TransactionHistoryItemMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Domain.Entities.Transaction, TransactionHistoryItemResponseDto>()
    .Map(dest => dest.TransactionId, src => src.Id)
    .Map(dest => dest.SenderIBAN, src => src.SenderAccount!.IBAN)
    .Map(dest => dest.SenderFullName, src => src.SenderAccount!.User!.FullName)
    .Map(dest => dest.ReceiverIBAN, src => src.ReceiverAccount!.IBAN)
    .Map(dest => dest.ReceiverFullName, src => src.ReceiverAccount!.User!.FullName)
    .Map(dest => dest.Status, src => src.Status.Name);
    }
}
