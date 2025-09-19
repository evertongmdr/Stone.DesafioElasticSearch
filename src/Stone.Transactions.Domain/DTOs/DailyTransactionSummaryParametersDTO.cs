namespace Stone.Transactions.Domain.DTOs
{
    public class DailyTransactionSummaryParametersDTO
    {
        public Guid ClientId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
