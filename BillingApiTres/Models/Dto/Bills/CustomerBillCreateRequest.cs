namespace BillingApiTres.Models.Dto
{
    public class CustomerBillCreateRequest
    {
        public List<long> BillIds { get; set; } = new List<long>();
    }
}
