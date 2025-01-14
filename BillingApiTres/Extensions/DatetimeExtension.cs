namespace BillingApiTres.Extensions
{
    public static class DatetimeExtension
    {
        /// <summary>
        /// 해당 일의 0시를 반환합니다
        /// </summary>
        public static DateTime StartOfDay(this DateTime dateTime)
        {
            return dateTime.Date;
        }

        /// <summary>
        /// 해당 일의 23시 59분 59초를 반환합니다
        /// </summary>
        public static DateTime EndOfDay(this DateTime dateTime)
        {
            return dateTime.Date.AddDays(1).AddSeconds(-1);
        }
    }
}
