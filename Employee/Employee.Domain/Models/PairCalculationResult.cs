namespace Employee.Domain.Models
{
    public class PairCalculationResult
    {
        /// <summary>
        /// The top pair (employees who worked together the longest)
        /// </summary>
        public PairResult TopPair { get; set; } = null!;

        /// <summary>
        /// All employee pairs sorted by total days (descending)
        /// </summary>
        public List<PairResult> AllPairs { get; set; } = new List<PairResult>();
    }
}
