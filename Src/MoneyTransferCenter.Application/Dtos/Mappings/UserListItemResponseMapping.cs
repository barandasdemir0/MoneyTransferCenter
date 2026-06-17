using Mapster;
using MoneyTransferCenter.Application.Dtos.Account;

namespace MoneyTransferCenter.Application.Dtos.Mappings;

public sealed class UserListItemResponseMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Domain.Entities.Account, UserListItemResponseDto>()
            .Map(dest => dest.FullName,
                 src => src.User != null
            ? src.User.FullName
            : "Bilinmiyor")

            .Map(dest => dest.Email,
         src => src.User != null
            ? src.User.Email
            : "Bilinmiyor")

            .Map(dest => dest.AccountStatus,
                    src => src.Status.Name);
    }
}
