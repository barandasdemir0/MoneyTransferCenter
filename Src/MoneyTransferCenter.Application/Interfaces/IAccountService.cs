using MoneyTransferCenter.Application.Dtos.Account;

namespace MoneyTransferCenter.Application.Interfaces;

public interface IAccountService
{
    Task<AccountResponseDto> CreateAccountForUserAsync(Guid userId);//otomatik iban ve hesap numarası oluşturulacak
    Task<AccountResponseDto> CompleteProfileAsync(Guid userId, CompleteProfileRequestDto request);
    Task<AccountResponseDto> GetMyAccountAsync(Guid userId);
    Task<List<UserListItemResponseDto>> GetAllUsersAsync();
}
