namespace Stone.Transactions.Domain.Entities
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public TransactionType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid ClientId { get; set; }
        public Guid PayerId { get; set; }
        public decimal Amount { get; set; }
    }
}
