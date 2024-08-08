using System.ComponentModel.DataAnnotations;

namespace Entity.models;

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
    public int Price { get; set; }

    [Required]
    public string Category { get; set; }

    public int Quantity { get; set; }

    public virtual ICollection<ProductInOrder> ProductInOrders { get; set; }
}
