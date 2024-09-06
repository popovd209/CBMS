using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.DTOs
{
    public class WaiterPerformanceDTO
    {
        public string WaiterName { get; set; }
        public int TotalOrdersServed { get; set; }
        public string MostCommonCategory { get; set; }
        public double TotalIncome { get; set; }
        public string Date { get; set; }
    }
}
