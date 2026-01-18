using CsvHelper;
using CsvHelper.Configuration;
using Employee.Domain.Models;
using Employee.Services.Infrastructure;
using Employee.Services.Services.Interfaces;
using System.Globalization;
using System.Text;

namespace Employee.Services.Services
{

    public class CsvParser : ICsvParser
    {
        public async Task<IEnumerable<EmployeeProjectRecord>> ParseAsync(Stream csvStream, CancellationToken cancellationToken = default)
        {
            if (csvStream == null || !csvStream.CanRead)
                throw new ArgumentException("Stream must be readable", nameof(csvStream));

            // Detect if CSV has header by reading first line
            bool hasHeader = await DetectHeaderAsync(csvStream, cancellationToken);

            // Reset stream position after detection
            csvStream.Position = 0;

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = hasHeader,
                MissingFieldFound = null,
                BadDataFound = null,
                TrimOptions = TrimOptions.Trim,
                IgnoreBlankLines = true,
                // Allow quoted fields
                Mode = CsvMode.RFC4180,
                // Handle both CRLF and LF
                DetectDelimiter = false,
                Delimiter = ",",
                // Treat empty strings as null
                HeaderValidated = null,
                PrepareHeaderForMatch = args => args.Header.Trim()
            };

            using var reader = new StreamReader(csvStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 4096, leaveOpen: true);
            using var csv = new CsvReader(reader, config);

            // Register appropriate ClassMap based on header detection
            if (hasHeader)
            {
                csv.Context.RegisterClassMap<EmployeeProjectRecordHeaderMap>();
            }
            else
            {
                csv.Context.RegisterClassMap<EmployeeProjectRecordMap>();
            }

            var records = new List<EmployeeProjectRecord>();

            try
            {
                // Use GetRecordsAsync for memory-efficient streaming
                await foreach (var record in csv.GetRecordsAsync<EmployeeProjectRecord>(cancellationToken))
                {
                    // Normalize DateTo: treat "NULL", empty, or whitespace as null
                    if (string.IsNullOrWhiteSpace(record.DateTo) ||
                        record.DateTo.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                    {
                        record.DateTo = null;
                    }

                    records.Add(record);
                }
            }
            catch (CsvHelper.HeaderValidationException hvex)
            {
                throw new FormatException($"CSV header validation failed: {hvex.Message}", hvex);
            }
            catch (CsvHelper.ReaderException rex)
            {
                throw new FormatException($"CSV parsing error at row {rex.Context?.Parser?.Row}: {rex.Message}", rex);
            }

            return records;
        }

        private static async Task<bool> DetectHeaderAsync(Stream stream, CancellationToken cancellationToken)
        {
            if (!stream.CanSeek)
                throw new ArgumentException("Stream must be seekable for header detection", nameof(stream));

            var originalPosition = stream.Position;

            try
            {
                using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);

                var firstLine = await reader.ReadLineAsync(cancellationToken);

                if (string.IsNullOrWhiteSpace(firstLine))
                    return false;

                // Detect header by checking if first line contains expected header names
                var upperLine = firstLine.ToUpperInvariant();
                return upperLine.Contains("EMPID") ||
                       upperLine.Contains("PROJECTID") ||
                       upperLine.Contains("DATEFROM") ||
                       upperLine.Contains("DATETO");
            }
            finally
            {
                // Always restore original position
                stream.Position = originalPosition;
            }
        }
    }
}
