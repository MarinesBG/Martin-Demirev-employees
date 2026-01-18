namespace Employee.Domain.Models
{
    public class ProjectDetail
    {
        public int EmployeeId1 { get; set; }
        public int EmployeeId2 { get; set; }
        public int ProjectId { get; set; }
        public int DaysWorked { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
