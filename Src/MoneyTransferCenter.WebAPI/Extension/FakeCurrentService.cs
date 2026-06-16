using MoneyTransferCenter.Application.Interfaces;

namespace MoneyTransferCenter.WebAPI.Extension;

public class FakeCurrentService : ICurrentUserService
{
    public Guid? UserId => Guid.Empty;
}
