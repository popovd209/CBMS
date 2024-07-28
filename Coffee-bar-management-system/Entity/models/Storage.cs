using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.models
{
    public class Storage
    {
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }
        public int Quanity { get; set; }
    }
}
