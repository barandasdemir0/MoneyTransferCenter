using FluentValidation;
using MoneyTransferCenter.Application.Dtos.Auth;

namespace MoneyTransferCenter.Application.Validators.Auth;

public sealed class LoginRequestValidator:AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
           .NotEmpty().WithMessage("E-posta adresi boş bırakılamaz.")
           .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre boş bırakılamaz.");
    }
}
