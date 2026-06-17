using MoneyTransferCenter.Domain.Entities;

namespace MoneyTransferCenter.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(AppUser user);
}
