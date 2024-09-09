using Admin_CBMS.Views.Waiters.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Admin_CBMS.Views.Waiters
{
    public class WaiterPerformanceViewModel
    {
        public string SelectedWaiterId { get; set; }
        public DateTime SelectedDate { get; set; }
        public IEnumerable<SelectListItem> Waiters { get; set; }
        public PerformanceResultDTO PerformanceResult { get; set; } = new PerformanceResultDTO();
    }
}
