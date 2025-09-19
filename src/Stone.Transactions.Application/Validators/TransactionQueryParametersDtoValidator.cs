using FluentValidation;
using Stone.Transactions.Domain.DTOs;

namespace Stone.Transactions.Application.Validators
{
    public class TransactionQueryParametersDtoValidator : AbstractValidator<TransactionQueryParametersDTO>
    {
        public TransactionQueryParametersDtoValidator()
        {
            RuleFor(q => q.ClientId)
                .NotEqual(Guid.Empty)
                .WithMessage("O cliente da transação deve ser informado.");

            // Validação das datas
            RuleFor(q => q)
                .Must(q =>
                    (!q.StartDate.HasValue && !q.EndDate.HasValue) ||
                    (q.StartDate.HasValue && q.EndDate.HasValue))
                .WithMessage("Se uma das datas for preenchida, a outra também deve ser informada.");
        }
    }
}
