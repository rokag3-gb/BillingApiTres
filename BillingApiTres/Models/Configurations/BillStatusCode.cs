namespace BillingApiTres.Models.Configurations
{
    public record BillStatusCode
    {
        public string Certain { get; set; } = "BST-EXT";
        public string Uncertain { get; set; } = "BST-001";
    }
}
