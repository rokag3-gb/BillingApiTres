namespace BillingApiTres.Models.Dto
{
    public record PaginationResponse<T>
    {
        public int CurrentPage { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public IEnumerable<T> Data { get; set; }

        public PaginationResponse(IEnumerable<T> data, int totalCount, int? offset, int? limit)
        {
            Data = data;
            TotalCount = totalCount;
            PageSize = limit ?? totalCount;
            var offsetValue = offset ?? totalCount;
            CurrentPage = (int)Math.Ceiling((double)TotalCount / offsetValue);
        }
    }
}
