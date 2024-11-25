
namespace BillingApiTres.Converters
{
    public interface ITimeZoneConverter
    {
        DateTime ConvertToLocal(DateTime source, string timezoneId);
        DateTime ConvertToUtc(DateTime original, string timeZoneId);
    }
}