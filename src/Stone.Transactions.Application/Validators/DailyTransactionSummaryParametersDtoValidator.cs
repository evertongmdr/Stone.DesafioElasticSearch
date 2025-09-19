using FluentValidation;
using Stone.Transactions.Domain.DTOs;

namespace Stone.Transactions.Application.Validators
{
    public class DailyTransactionSummaryParametersDtoValidator : AbstractValidator<DailyTransactionSummaryParametersDTO>
    {
        public DailyTransactionSummaryParametersDtoValidator()
        {
            RuleFor(q => q.ClientId)
                .NotEqual(Guid.Empty).WithMessage("O cliente da transação deve ser informado.");


            RuleFor(q => q.StartDate)
                .NotEqual(DateTime.MinValue).WithMessage("A data de inicio deve ser informado.")
                .NotEqual(DateTime.MaxValue).WithMessage("A data de inicio informada é inválida.");


            RuleFor(q => q.EndDate)
               .NotEqual(DateTime.MinValue).WithMessage("A data final deve ser informado.")
               .NotEqual(DateTime.MaxValue).WithMessage("A data final informada é inválida.");
        }
    }
}
