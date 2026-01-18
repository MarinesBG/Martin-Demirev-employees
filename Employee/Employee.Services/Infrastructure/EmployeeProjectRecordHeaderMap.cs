using CsvHelper.Configuration;
using Employee.Domain.Models;

namespace Employee.Services.Infrastructure
{
    public sealed class EmployeeProjectRecordHeaderMap : ClassMap<EmployeeProjectRecord>
    {
        public EmployeeProjectRecordHeaderMap()
        {
            // Name-based mapping for CSV files with headers
            Map(m => m.EmployeeID).Name("EmpID");
            Map(m => m.ProjectID).Name("ProjectID");
            Map(m => m.DateFrom).Name("DateFrom");
            Map(m => m.DateTo).Name("DateTo").Default(null);
        }
    }
}
