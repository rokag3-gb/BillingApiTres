using System.Collections.ObjectModel;
using System.Net;

namespace BillingApiTres.Models.Dto
{
    public record MultiHttpCodeResponse
    {
        private List<CodeResponse> _fails = new List<CodeResponse>();

        public List<BillResponse> Success { get; set; } = new List<BillResponse>();
        public ReadOnlyCollection<CodeResponse> Fails { get; private set; } = ReadOnlyCollection<CodeResponse>.Empty;

        public void AddFail(int httpStatusCode, IEnumerable<long> ids)
        {
            
            var fail = _fails.FirstOrDefault(f => f.HttpCode == httpStatusCode);
            if (fail == null)
                _fails.Add(new CodeResponse(httpStatusCode, ids));
            else
            {
                fail.EntityIds.AddRange(ids);
                fail.EntityIds.Distinct();
            }

            Fails = new ReadOnlyCollection<CodeResponse>(_fails);
        }

        public void AddFail(IEnumerable<CodeResponse> fails)
        {
            foreach (var fail in fails)
            {
                AddFail(fail.HttpCode, fail.EntityIds);
            }
        }
    }

    public record CodeResponse
    {
        public CodeResponse(int statusCode, IEnumerable<long> ids)
        {
            HttpCode = statusCode;
            EntityIds = ids.ToList();
        }

        public int HttpCode { get; set; }
        public List<long> EntityIds { get; set; } = new List<long>();
    }
}
