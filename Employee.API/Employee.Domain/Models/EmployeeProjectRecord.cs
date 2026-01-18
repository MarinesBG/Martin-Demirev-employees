namespace Employee.Domain.Models
{
    public class EmployeeProjectRecord
    {
        public int EmployeeID { get; set; }
        public int ProjectID { get; set; }
        public string DateFrom { get; set; } = string.Empty;
        public string? DateTo { get; set; }
    }
}
