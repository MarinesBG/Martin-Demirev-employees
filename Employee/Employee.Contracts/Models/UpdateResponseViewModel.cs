namespace Employee.Contracts.Models
{
    public class UpdateResponseViewModel
    {
        /// <summary>
        /// THE top pair - employees who worked together the longest
        /// </summary>
        public PairResultViewModel? TopPair { get; set; }

        /// <summary>
        /// All pairs sorted by total days worked together (descending)
        /// </summary>
        public IEnumerable<PairResultViewModel> AllPairs { get; set; } = new List<PairResultViewModel>();

        /// <summary>
        /// Total number of unique employee pairs found
        /// </summary>
        public int TotalPairsFound { get; set; }

        /// <summary>
        /// Success message
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
