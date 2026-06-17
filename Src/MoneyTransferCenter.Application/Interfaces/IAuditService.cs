using MoneyTransferCenter.Domain.Entities;

namespace MoneyTransferCenter.Application.Interfaces;

public interface IAuditService
{
    Task LogUserRegisteredAsync(AppUser user);
    Task LogUserLoggedInAsync(AppUser user);
    Task LogLoginFailedAsync(AppUser user, string reason);
    Task LogAccountCreatedAsync(Guid userId, string iban);
    Task LogProfileCompletedAsync(Guid userId);
    Task LogAccountActivatedAsync(Guid userId);

    Task LogMoneyDepositedAsync(Guid userId, Guid accountId, string iban, decimal amount, decimal newBalance);

}
