namespace Employee.Services.Services.Interfaces
{
    public interface IDateParserService
    {
        /// <summary>
        /// Tries to parse a date string
        /// </summary>
        /// <param name="dateString">Date string to parse (can be null or "NULL")</param>
        /// <param name="treatNullAsToday">If true, null/NULL values return DateTime.Today</param>
        /// <param name="result">Parsed DateTime value</param>
        /// <returns>True if parsing succeeded</returns>
        bool TryParse(string? dateString, bool treatNullAsToday, out DateTime result);
    }
}
