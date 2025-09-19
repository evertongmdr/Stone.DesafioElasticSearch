using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Stone.Common.Core.DTOs.Support;
using Stone.Common.Core.Notifications;
using Stone.Common.Services.API.Controllers;
using Stone.Transactions.Application.Interfaces;
using Stone.Transactions.Application.Validators;
using Stone.Transactions.Domain.DTOs;
using Stone.Transactions.Domain.Entities;

namespace Stone.Transactions.API.Controllers
{
    public class TransactionController : MainController
    {

        private readonly ITransactionService _transactionService;

        public TransactionController(NotificationContext notificationContext, ITransactionService transactionService) : base(notificationContext)
        {
            _transactionService = transactionService;
        }

        [HttpGet("transactions")]
        [ProducesResponseType(typeof(PagedResult<Transaction>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]

        public async Task<IActionResult> GetPagedTransactions([FromQuery] TransactionQueryParametersDTO queryParametersDTO)
        {
            var validator = new TransactionQueryParametersDtoValidator();

            var validatorResult = await validator.ValidateAsync(queryParametersDTO);

            if (!validatorResult.IsValid)
                return CustomResponse(validatorResult);

            return CustomResponse(await _transactionService.GetPagedTransactionsAsync(queryParametersDTO));
        }


        [HttpGet("transactions/GetDailyTotals")]
        [ProducesResponseType(typeof(List<DailyTransactionSummaryDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]

        public async Task<IActionResult> GetDailyTotals([FromQuery] DailyTransactionSummaryParametersDTO queryParametersDTO)
        {
            var validator = new DailyTransactionSummaryParametersDtoValidator();

            var validatorResult = await validator.ValidateAsync(queryParametersDTO);

            if (!validatorResult.IsValid)
                return CustomResponse(validatorResult);

            return CustomResponse(await _transactionService.GetDailyTotalsAsync(queryParametersDTO));
        }
    }
}
