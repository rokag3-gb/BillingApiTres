using System.Globalization;

namespace BillingApiTres.Converters
{
    public record CurrencyInfo
    {
        public string CurrencyCode { get; set; }
        public string CurrencySymbol { get; set; }
        public string CurrencyEnglishName { get; set; }
        public string CurrencyNativeName { get; set; }
        public List<string> Countries { get; set; }
        public int DecimalDigits { get; set; }
    }

    public class CurrencyConverter(ILogger<CurrencyConverter> logger)
    {
        public CurrencyInfo? GetCurrencyInfo(string currencyCode)
        {
            try
            {
                var cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                    .Where(c => new RegionInfo(c.Name).ISOCurrencySymbol == currencyCode)
                    .ToList();

                if (!cultures.Any())
                    return null;

                var firstCulture = cultures.First();
                var region = new RegionInfo(firstCulture.Name);

                return new CurrencyInfo
                {
                    CurrencyCode = currencyCode,
                    CurrencySymbol = region.CurrencySymbol,
                    CurrencyEnglishName = region.CurrencyEnglishName,
                    CurrencyNativeName = region.CurrencyNativeName,
                    Countries = cultures.Select(c => new RegionInfo(c.Name).EnglishName).Distinct().ToList(),
                    DecimalDigits = GetDecimalDigits(firstCulture)
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, null);
                return null;
            }
        }

        private static int GetDecimalDigits(CultureInfo culture)
        {
            return culture.NumberFormat.CurrencyDecimalDigits;
        }
    }
}
