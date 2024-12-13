using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BillingApiTres.Models.Dto
{
    public enum UsageUnit
    {
        [EnumMember(Value = "monthly")]
        Monthly,
        [EnumMember (Value = "daily")]
        Daily
    }

    public record UsageRequest
    {
        /// <summary>
        /// 사용량 단위. 월단위, 일단위...
        /// </summary>
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UsageUnit UsageUnit { get; set; }

        /// <summary>
        /// 사용량 조회 대상의 account id. csv 형식.
        /// </summary>
        [Required]
        public string AccountIds { get; set; }

        [Required]
        public DateTime From { get; set; }

        [Required]
        public DateTime To { get; set; }

        public int? Offset { get; set; }
        public int? Limit { get; set; }
    }
}
