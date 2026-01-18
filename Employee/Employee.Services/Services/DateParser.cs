using Employee.Services.Services.Interfaces;
using System.Globalization;

namespace Employee.Services.Services
{
    public class DateParser : IDateParser
    {
        private static readonly string[] SupportedFormats = new[]
       {
            "yyyy-MM-dd",
            "yyyy/MM/dd",
            "dd-MM-yyyy",
            "dd/MM/yyyy",
            "MM/dd/yyyy",
            "MM-dd-yyyy",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy/MM/dd HH:mm:ss"
        };

        public bool TryParse(string? dateString, bool treatNullAsToday, out DateTime result)
        {
            // Handle null or "NULL" string
            if (string.IsNullOrWhiteSpace(dateString) ||
                dateString.Equals("NULL", StringComparison.OrdinalIgnoreCase))
            {
                if (treatNullAsToday)
                {
                    result = DateTime.Today;
                    return true;
                }

                result = default;
                return false;
            }

            // Try parsing with supported formats
            if (DateTime.TryParseExact(
                dateString.Trim(),
                SupportedFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out result))
            {
                return true;
            }

            // Fallback to general parse
            return DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
        }
    }
}