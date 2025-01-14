using System.ComponentModel.DataAnnotations;

namespace BillingApiTres.Models.Dto
{
    public record BillListRequest
    {
        /// <summary>
        /// 검색 기간
        /// </summary>
        [Required]
        public DateTime From { get; set; }

        /// <summary>
        /// 검색 기간
        /// </summary>
        [Required]
        public DateTime To { get; set; }

        /// <summary>
        /// 판매사 또는 구매사의 account Id. csv 형식
        /// </summary>
        [Required]
        public string AccountIds { get; set; }

        /// <summary>
        /// 조회 시작 레코드 인덱스. 없으면 전체 레코드를 조회.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int? Offset { get; set; }

        /// <summary>
        /// 최대 조회 개수. 없으면 전체 레코드를 조회.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int? limit { get; set; }
    }
}
