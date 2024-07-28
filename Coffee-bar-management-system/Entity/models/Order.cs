using Entity.models.identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.models
{
    public class Order : BaseEntity
    {
        public int Total { get; set; }
        public int TableTag { get; set; }
        public enum State
        {
            NEW,
            IN_PROGRESS,
            DONE,
            COMPLETE,
            DELIVERED,
            CANCELLED,
            PAID
        }
        public ICollection<ProductInOrder> ProductsInOrder { get; set; }
        public User? CreatedBy { get; set; }
        public DateTime CreatedWhen { get; set; }
    }
}
