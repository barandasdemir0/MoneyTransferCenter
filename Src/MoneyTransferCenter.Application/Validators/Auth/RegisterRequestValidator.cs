using FluentValidation;
using MoneyTransferCenter.Application.Dtos.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoneyTransferCenter.Application.Validators.Auth;

public sealed class RegisterRequestValidator:AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FirstName)
           .NotEmpty().WithMessage("Ad alanı boş bırakılamaz.")
           .MinimumLength(2).WithMessage("Ad en az 2 karakter olmalıdır.")
           .MaximumLength(50).WithMessage("Ad en fazla 50 karakter olabilir.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad alanı boş bırakılamaz.")
            .MinimumLength(2).WithMessage("Soyad en az 2 karakter olmalıdır.")
            .MaximumLength(50).WithMessage("Soyad en fazla 50 karakter olabilir.");

        RuleFor(x => x.NationalId)
            .NotEmpty().WithMessage("TC Kimlik numarası boş bırakılamaz.")
            .Length(11).WithMessage("TC Kimlik numarası 11 haneli olmalıdır.")
            .Matches("^[0-9]+$").WithMessage("TC Kimlik numarası sadece rakamlardan oluşmalıdır.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi boş bırakılamaz.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre boş bırakılamaz.")
            .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.")
            .Matches("[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir.")
            .Matches("[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir.")
            .Matches("[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir.")
            .Matches("[0-9]").WithMessage("Şifre en az bir rakam içermelidir.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Şifre tekrarı boş bırakılamaz.")
            .Equal(x => x.Password).WithMessage("Şifreler eşleşmiyor.");
    }

}
