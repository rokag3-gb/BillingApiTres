using System.ComponentModel.DataAnnotations;

namespace BillingApiTres.Models.Dto.Dashboards
{
    public record RecentThreeMonthRequest
    {
        [Required]
        public DateOnly RequestDate { get; set; }

        [Required]
        public string AccountIdsCsv { get; set; }
    }
}
