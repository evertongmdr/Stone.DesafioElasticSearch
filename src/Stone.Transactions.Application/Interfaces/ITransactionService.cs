using Stone.Common.Core.DTOs.Support;
using Stone.Transactions.Domain.DTOs;
using Stone.Transactions.Domain.Entities;

namespace Stone.Transactions.Application.Interfaces
{
    public interface ITransactionService
    {
        Task<PagedResult<Transaction>> GetPagedTransactionsAsync(TransactionQueryParametersDTO parameters);
        Task<List<DailyTransactionSummaryDTO>> GetDailyTotalsAsync(DailyTransactionSummaryParametersDTO parameters);
    }
}
