namespace Employee.Domain.Models
{
    internal class EmployeeProjectRecord
    {
        public int EmpID { get; set; }
        public int ProjectID { get; set; }
        public string DateFrom { get; set; } = string.Empty;
        public string? DateTo { get; set; }
    }
}
