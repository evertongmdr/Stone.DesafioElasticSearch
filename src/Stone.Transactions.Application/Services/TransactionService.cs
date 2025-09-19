using Stone.Common.Core.DTOs.Support;
using Stone.Transactions.Application.Interfaces;
using Stone.Transactions.Domain.DTOs;
using Stone.Transactions.Domain.Entities;
using Stone.Transactions.Domain.Interfaces.Services;

namespace Stone.Transactions.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactoinSearchEngine _transactionElasticSearchService;

        public TransactionService(ITransactoinSearchEngine transactionElasticSearchService)
        {
            _transactionElasticSearchService = transactionElasticSearchService;
        }



        public async Task<PagedResult<Transaction>> GetPagedTransactionsAsync(TransactionQueryParametersDTO parameters)
        {
            var transactions = await _transactionElasticSearchService.GetTransactionsAsync(parameters);

            return new PagedResult<Transaction>(
                transactions,
                transactions.Count,
                parameters.CurrentPage,
                parameters.PageSize
            );
        }

        public async Task<List<DailyTransactionSummaryDTO>> GetDailyTotalsAsync(DailyTransactionSummaryParametersDTO parameters)
        {
            return await _transactionElasticSearchService.GetDailyTotalsAsync(parameters);
        }
    }
}

//TODO: analisar se da pra implementar o Dispose.