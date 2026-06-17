using MoneyTransferCenter.Domain.Entities;

namespace MoneyTransferCenter.Application.Interfaces;

public interface IAuditService
{
    Task LogUserRegisteredAsync(AppUser user);
    Task LogUserLoggedInAsync(AppUser user);
    Task LogLoginFailedAsync(AppUser user, string reason);
}
