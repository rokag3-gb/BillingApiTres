namespace BillingApiTres.Models.Dto
{
    public record PaginationResponse<T>
    {
        public int CurrentPage { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public IEnumerable<T> Data { get; set; }

        public PaginationResponse()
        {
            Data = Enumerable.Empty<T>();
            TotalCount = 0;
            PageSize = 1;
            CurrentPage = 1;
        }

        public PaginationResponse(IEnumerable<T> data, int totalCount, int? offset, int? limit)
        {
            Data = data;
            TotalCount = totalCount;
            var offsetValue = offset ?? totalCount;

            if (totalCount == 0)
            {
                CurrentPage = 1;
                PageSize = 1;
                offsetValue = 0;
            }
            else
            {
                PageSize = limit ?? totalCount;
                CurrentPage = (int)Math.Ceiling((double)TotalCount / offsetValue);
            }
        }
    }
}
