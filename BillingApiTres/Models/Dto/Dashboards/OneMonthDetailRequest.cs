using System.ComponentModel.DataAnnotations;

namespace BillingApiTres.Models.Dto.Dashboards
{
    public class OneMonthDetailRequest
    {
        [Required]
        public DateOnly RequestDate { get; set; }

        [Required]
        public string AccountIdsCsv { get; set; } = string.Empty;
    }
}
