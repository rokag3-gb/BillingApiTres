namespace BillingApiTres.Models.Configurations
{
    public record CloudVendorCode
    {
        public string NCP { get; set; } = "VEN-NCP";
        public string Azure { get; set; } = "VEN-AZT";
        public string AWS { get; set; } = "VEN-AWS";
        public string GC { get; set; } = "VEN-GCP";
    }
}
