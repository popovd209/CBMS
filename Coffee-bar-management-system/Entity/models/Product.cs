using System.ComponentModel.DataAnnotations;

namespace Entity.models;

public class Product : BaseEntity
{
    public Product()
    {
        ProductInOrders = [];
    }

    [Required]
    public string Name { get; set; }

    [Required]
    public int Price { get; set; }

    [Required]
    public string Category { get; set; }

    public virtual ICollection<ProductInOrder> ProductInOrders { get; set; }
}
