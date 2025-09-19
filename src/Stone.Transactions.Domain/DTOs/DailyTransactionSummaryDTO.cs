namespace Stone.Transactions.Domain.DTOs
{
    public class DailyTransactionSummaryDTO
    {
        public DateTime Date { get; set; }
        public string TransactionType { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
