using Microsoft.AspNetCore.Mvc.Rendering;

namespace Admin_CBMS.Models.WaiterPerformance
{
    public class WaiterPerformanceViewModel
    {
        public string SelectedWaiterId { get; set; }
        public DateTime SelectedDate { get; set; }
        public SelectList Waiters { get; set; } // Dropdown list for waiters
        public List<DTOs.PerformanceResult> PerformanceResults { get; set; } // Results to be shown after search
    }
}
