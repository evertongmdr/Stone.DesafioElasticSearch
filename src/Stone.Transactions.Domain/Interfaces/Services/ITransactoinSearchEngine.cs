using Stone.Transactions.Domain.DTOs;
using Stone.Transactions.Domain.Entities;

namespace Stone.Transactions.Domain.Interfaces.Services
{
    public interface ITransactoinSearchEngine
    {
        Task<List<Transaction>> GetTransactionsAsync(TransactionQueryParametersDTO parameters);

        Task<List<DailyTransactionSummaryDTO>> GetDailyTotalsAsync(DailyTransactionSummaryParametersDTO parameters);

    }
}