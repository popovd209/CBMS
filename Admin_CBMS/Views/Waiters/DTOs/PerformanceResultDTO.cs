namespace Admin_CBMS.Views.Waiters.DTOs
{
    public class PerformanceResultDTO
    {
        public PerformanceResultDTO()
        {
        }
        public string? WaiterName { get; set; }
        public int? TotalOrdersServed { get; set; }
        public string? MostCommonCategory { get; set; }
        public double? TotalIncome { get; set; }
        public DateTime? Date { get; set; }
    }
}
