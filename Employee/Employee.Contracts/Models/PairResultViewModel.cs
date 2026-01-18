namespace Employee.Contracts.Models
{
    public class PairResultViewModel
    {
        public int EmployeeIdA { get; set; }
        public int EmployeeIdB { get; set; }
        public int TotalDays { get; set; }
        public IEnumerable<PairProjectViewModel> Projects { get; set; } = new List<PairProjectViewModel>();
    }
}
