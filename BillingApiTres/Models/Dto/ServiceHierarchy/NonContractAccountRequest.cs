using System.ComponentModel.DataAnnotations;

namespace BillingApiTres.Models.Dto
{
    public class NonContractAccountRequest
    {
        /// <summary>
        /// 데이터 조회 시작 시퀀스 값을 설정합니다. 없으면 전체 데이터를 조회합니다.
        /// </summary>
        [Range(0, long.MaxValue)]
        public int? Offset { get; set; }

        /// <summary>
        /// 조회 데이터 수를 설정합니다. 없으면 전체 데이터를 조회합니다.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int? Limit { get; set; }
    }
}
