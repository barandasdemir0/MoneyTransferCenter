using FluentValidation;
using MoneyTransferCenter.Application.Dtos.Transaction.Transfer;

namespace MoneyTransferCenter.Application.Validators.Transaction;

public class TransferRequestValidator:AbstractValidator<TransferRequestDto>
{
    public TransferRequestValidator()
    {

        RuleFor(x => x.ReceiverIBAN)
            .NotEmpty().WithMessage("Alıcı IBAN boş olamaz.")
            .Length(26).WithMessage("IBAN 26 karakter olmalıdır.")
            .Matches("^TR").WithMessage("Sadece TR IBAN desteklenmektedir.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Transfer tutarı sıfırdan büyük olmalıdır.")
            .LessThanOrEqualTo(50_000).WithMessage("Tek seferde en fazla 50.000 TL transfer yapılabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(250).WithMessage("Açıklama en fazla 250 karakter olabilir.")
            .When(x => x.Description != null);
    }
}
