
namespace BillingApiTres.Converters
{
    public interface ITimeZoneConverter
    {
        DateTime ConvertToSource(DateTime source, string timezoneId);
        DateTime ConvertToUtc(DateTime original, string timeZoneId);
    }
}