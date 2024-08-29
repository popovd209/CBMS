using Entity.Models;

namespace Service.Interface;

public interface IServerService
{
    ICollection<Order> GetPersonalizedOrdersByState(State state, String id);
    Order CreateOrder(Order order, String userId);
    void AddProductToOrder(Order order, Product product, int quantity);
    Order ChangeOrderState(Order order, State state);
    Order GetOrderDetails(Guid? id);
    void CancelOrder(Order order);
}