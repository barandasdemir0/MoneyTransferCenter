using FluentValidation;
using MoneyTransferCenter.Application.Dtos.Account;

namespace MoneyTransferCenter.Application.Validators.Account;

public class CompleteProfileValidator:AbstractValidator<CompleteProfileRequestDto>
{
    public CompleteProfileValidator()
    {
        RuleFor(x => x.Address)
           .NotEmpty().WithMessage("Adres boş bırakılamaz.")
           .MaximumLength(250).WithMessage("Adres en fazla 250 karakter olabilir.");
        RuleFor(x => x.City)
            .NotEmpty().WithMessage("Şehir boş bırakılamaz.")
            .MaximumLength(50).WithMessage("Şehir en fazla 50 karakter olabilir.");
        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("Posta kodu boş bırakılamaz.")
            .Matches("^[0-9]{5}$").WithMessage("Posta kodu 5 haneli olmalıdır.");
        RuleFor(x => x.TelephoneNumber)
            .NotEmpty().WithMessage("Telefon numarası boş bırakılamaz.")
            .Matches("^[0-9]{10,15}$").WithMessage("Geçerli bir telefon numarası giriniz.");
    }
}
