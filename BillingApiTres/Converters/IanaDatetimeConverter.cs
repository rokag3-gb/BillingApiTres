using TimeZoneConverter;

namespace BillingApiTres.Converters
{
    public class IanaDatetimeConverter
    {
        public DateTime ConvertToUtc(DateTime original, string ianaId)
        {
            var datetime = DateTime.UtcNow;

            TimeZoneInfo timeZoneInfo = TZConvert.GetTimeZoneInfo(ianaId);
            datetime = TimeZoneInfo.ConvertTimeToUtc(original, timeZoneInfo);

            return datetime;
        }

        public DateTime ConvertToSource(DateTime source, string ianaId)
        {
            var datetime = DateTime.UtcNow;

            TimeZoneInfo timeZoneInfo = TZConvert.GetTimeZoneInfo(ianaId);
            datetime = TimeZoneInfo.ConvertTimeFromUtc(source, timeZoneInfo);

            return datetime;
        }
    }
}
