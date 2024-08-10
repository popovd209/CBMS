using Entity.Models.Identity;

namespace Entity.Models;

public class Order : BaseEntity
{
    public Order()
    {
        ProductsInOrder = [];
        OrderState = State.NEW;
    }

    public int Total { get; set; }

    public int TableTag { get; set; }

    public State OrderState { get; set; }

    public virtual ICollection<ProductInOrder> ProductsInOrder { get; set; }

    public string? CreatedById { get; set; }

    public virtual CbmsUser? CreatedBy { get; set; }

    public DateTime CreatedWhen { get; set; }
}
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