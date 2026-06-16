namespace MoneyTransferCenter.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
}
