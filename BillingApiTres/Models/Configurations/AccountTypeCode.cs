namespace BillingApiTres.Models.Configurations
{
    /// <summary>
    /// 고객사 타입 코드에 대한 구성 요소의 값을 나타냅니다 
    /// </summary>
    public record AccountTypeCode
    {
        public string Acme { get; init; } = "SHT-ROT";
        public string Partner { get; init; } = "SHT-PRT";
        public string Customer { get; init; } = "SHT-CUS";
    }
}
