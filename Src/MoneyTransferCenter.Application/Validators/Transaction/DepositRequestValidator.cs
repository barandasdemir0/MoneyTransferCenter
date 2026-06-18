using FluentValidation;
using MoneyTransferCenter.Application.Dtos.Transaction.Deposit;

namespace MoneyTransferCenter.Application.Validators.Transaction;

public sealed class DepositRequestValidator:AbstractValidator<DepositRequestDto>
{
    public DepositRequestValidator()
    {
        RuleFor(x => x.Amount)
          .GreaterThan(0).WithMessage("Yatırılacak tutar sıfırdan büyük olmalıdır.")
          .LessThanOrEqualTo(100_000).WithMessage("Tek seferde en fazla 100.000 TL yatırılabilir.");
    }
}
