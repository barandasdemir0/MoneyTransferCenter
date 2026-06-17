using Mapster;
using MoneyTransferCenter.Application.Dtos.Account;


namespace MoneyTransferCenter.Application.Dtos.Mappings;

public sealed class AccountMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Domain.Entities.Account, AccountResponseDto>()
           .Map(dest => dest.Status,
                src => src.Status.Name)

           .Map(dest => dest.OwnerFullName,
                src => src.User != null
                    ? src.User.FullName
                    : "Bilinmiyor")

           .Map(dest => dest.OwnerEmail,
                src => src.User != null
                    ? src.User.Email
                    : "Bilinmiyor")

            .Map(dest => dest.IBAN,
                src => src.IBAN)

            .Map(dest => dest.Status,
                src => src.Status.Name)

            .Map(dest => dest.CreatedAt,
                src => src.CreatedAt);
    }
}
