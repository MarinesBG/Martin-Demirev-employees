using CsvHelper.Configuration;
using Employee.Domain.Models;

namespace Employee.Services.Infrastructure
{
    public sealed class EmployeeProjectRecordMap : ClassMap<EmployeeProjectRecord>
    {
        public EmployeeProjectRecordMap()
        {
            // Index-based mapping for CSV files without headers
            Map(m => m.EmployeeID).Index(0);
            Map(m => m.ProjectID).Index(1);
            Map(m => m.DateFrom).Index(2);
            Map(m => m.DateTo).Index(3).Default(null);
        }
    }
}
