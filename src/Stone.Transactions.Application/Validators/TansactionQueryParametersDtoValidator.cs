using FluentValidation;
using Stone.Transactions.Domain.DTOs;

namespace Stone.Transactions.Application.Validators
{
    public class TansactionQueryParametersDtoValidator : AbstractValidator<TransactionQueryParametersDTO>
    {
        public TansactionQueryParametersDtoValidator()
        {
            RuleFor(q => q.ClientId)
                .NotEqual(Guid.Empty).WithMessage("O cliente da transação deve ser informado.");
        }
    }
}
