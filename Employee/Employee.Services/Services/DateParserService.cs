using Employee.Services.Services.Interfaces;
using System.Globalization;

namespace Employee.Services.Services
{
    public class DateParserService : IDateParserService
    {
        private static readonly string[] SupportedFormats = new[]
        {
            "yyyy-MM-dd",
            "yyyy/MM/dd",
            "yyyy.MM.dd",
            "yyyyMMdd",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ssZ",
            "yyyy-MM-ddTHH:mm:ssK",
            "yyyy-MM-ddTHH:mm:ss.fffZ",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy/MM/dd HH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss.fff",
            "dd-MM-yyyy",
            "dd/MM/yyyy",
            "dd.MM.yyyy",
            "ddMMyyyy",
            "dd-MM-yy",
            "dd/MM/yy",
            "MM-dd-yyyy",
            "MM/dd/yyyy",
            "MM.dd.yyyy",
            "MM/dd/yy",
            "dd MMM yyyy",
            "dd MMMM yyyy",
            "dd-MMM-yyyy",
            "MMM dd, yyyy",
            "MMMM dd, yyyy",
            "dd MMM yyyy HH:mm",
            "dd MMM yyyy HH:mm:ss",
            "dd.MM.yy",
            "yyyy MM dd"
        };

        public bool TryParse(string? dateString, bool treatNullAsToday, out DateTime result)
        {
            // Handle null or "NULL" string
            if (string.IsNullOrWhiteSpace(dateString) ||
                dateString.Equals("NULL", StringComparison.OrdinalIgnoreCase))
            {
                if (treatNullAsToday)
                {
                    result = DateTime.UtcNow.Date;
                    return true;
                }

                result = default;
                return false;
            }

            var s = dateString.Trim();

            // 1) Try parsing assuming UTC / respecting explicit timezones -> result will be UTC
            var utcStyles = DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;
            if (DateTime.TryParseExact(s, SupportedFormats, CultureInfo.InvariantCulture, utcStyles, out result))
                return true;
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, utcStyles, out result))
                return true;

            // 2) Fallback: try parsing as local time, then convert to UTC (AdjustToUniversal does conversion)
            var localStyles = DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal | DateTimeStyles.AdjustToUniversal;
            if (DateTime.TryParseExact(s, SupportedFormats, CultureInfo.InvariantCulture, localStyles, out result))
                return true;
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, localStyles, out result))
                return true;

            // Could not parse
            result = default;
            return false;
        }
    }
}
