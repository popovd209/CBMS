using Entity.Models;
using Repository.Interface;
using Service.Interface;

namespace Service.Implementation;

public class BartenderService : IBartenderService
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IOrderRepository _detailedOrderRepository;

    public BartenderService(IRepository<Order> orderRepository, IOrderRepository detailedOrderRepository)
    {
        _orderRepository = orderRepository;
        _detailedOrderRepository = detailedOrderRepository;
    }

    public ICollection<Order> GetFilteredOrdersByState(State state)
    {
        return _detailedOrderRepository.GetAllOrders()
            .Where(i => i.OrderState == state)
            .ToList();
    }

    public Order GetOrderById(Guid? id)
    {
        return _detailedOrderRepository.GetOrderDetails(id);
    }

    public void ChangeOrderStatus(Order order, State state)
    {
        order.OrderState = state;
        _orderRepository.Update(order);
    }

    public void MakeOrder(Order order)
    {
        var productsInOrder = order.ProductsInOrder;
        foreach (var product in productsInOrder)
        {
            product.Done = true;
        }
        
        order.OrderState = State.COMPLETE;
        _orderRepository.Update(order);
    }
}