using Stone.Common.Core.DTOs.Support;

namespace Stone.Transactions.Domain.DTOs
{
    public class TransactionQueryParametersDTO : QueryParameters
    {
        public Guid ClientId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
