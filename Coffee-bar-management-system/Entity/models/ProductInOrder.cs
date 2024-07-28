using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.models
{
    public class ProductInOrder
    {
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }
        public Guid OrderId { get; set; }
        public Order? Order { get; set; }
        public int Quanity { get; set; }
    }
}
