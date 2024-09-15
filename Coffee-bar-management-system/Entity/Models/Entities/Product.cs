using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Entity.Models
{
    public class Product : BaseEntity
    {
        public Product()
        {
            ProductInOrders = [];
            Quantity = 0;
        }

        [Required]
        public string Name { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:C0}")]
        public int Price { get; set; }

        [Required]
        public string Category { get; set; }

        public int Quantity { get; set; }

        public virtual ICollection<ProductInOrder> ProductInOrders { get; set; }
    }
}