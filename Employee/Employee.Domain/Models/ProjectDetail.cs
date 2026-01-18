namespace Employee.Domain.Models
{
    public class ProjectDetail
    {
        public int ProjectId { get; set; }
        public int DaysWorked { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
