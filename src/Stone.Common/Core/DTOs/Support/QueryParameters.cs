namespace Stone.Common.Core.DTOs.Support
{
    public class QueryParameters
    {
        private const int MaxPageSize = 1000;

        public int CurrentPage { get; set; } = 1;

        private int _pageSize = 1000;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }
}
