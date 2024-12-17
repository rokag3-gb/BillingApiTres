using System.ComponentModel.DataAnnotations;

namespace BillingApiTres.Models.Dto.Dashboards
{
    public record RecentTwelveMonthRequest
    {
        [Required]
        public DateOnly RequestDate { get; set; }

        [Required]
        public string AccountIdsCsv { get; set; }
    }
}
