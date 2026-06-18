using FluentValidation;
using MoneyTransferCenter.Application.Dtos.Transaction;
using MoneyTransferCenter.Domain.Enums;

namespace MoneyTransferCenter.Application.Validators.Transaction;

public class TransactionHistoryRequestValidator:AbstractValidator<TransactionHistoryRequestDto>
{
    public TransactionHistoryRequestValidator()
    {
        RuleFor(x => x.Filter)
           .Must(f => f is TransactionHistoryFilter.All or TransactionHistoryFilter.Sent or TransactionHistoryFilter.Received)
           .WithMessage("Filtre değeri 'all', 'sent' veya 'received' olmalıdır.");
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Sayfa numarası en az 1 olmalıdır.");
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Sayfa boyutu 1 ile 100 arasında olmalıdır.");
    }
}
