using Entity.Models;

namespace Service.Interface;

public interface IBartenderService
{
    ICollection<Order> GetFilteredOrdersByState(State state);
    Order GetOrderById(Guid? id);
    void ChangeOrderStatus(Order order, State state);
    void MakeOrder(Order order);
}