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
            if (dateTime.TimeOfDay == new TimeSpan(12, 0, 0))
                return dateTime.Date.AddDays(1).AddTicks(-1);
            return dateTime;
        }
    }
}
