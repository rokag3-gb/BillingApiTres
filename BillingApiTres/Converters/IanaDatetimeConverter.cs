using TimeZoneConverter;

namespace BillingApiTres.Converters
{
    public class IanaDatetimeConverter : ITimeZoneConverter
    {
        public DateTime ConvertToUtc(DateTime original, string timeZoneId)
        {
            var datetime = DateTime.UtcNow;

            TimeZoneInfo timeZoneInfo = TZConvert.GetTimeZoneInfo(timeZoneId);
            datetime = TimeZoneInfo.ConvertTimeToUtc(original, timeZoneInfo);

            return datetime;
        }

        public DateTime ConvertToLocal(DateTime source, string timezoneId)
        {
            var datetime = DateTime.UtcNow;

            TimeZoneInfo timeZoneInfo = TZConvert.GetTimeZoneInfo(timezoneId);
            datetime = TimeZoneInfo.ConvertTimeFromUtc(source, timeZoneInfo);

            return datetime;
        }
    }
}
