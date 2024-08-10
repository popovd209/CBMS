namespace Entity.Models;

public class Storage
{
    public Guid ProductId { get; set; }

    public virtual Product Product { get; set; }

    public int Quanity { get; set; }
}
