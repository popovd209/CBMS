using Entity.Models;
using Repository.Interface;
using Service.Interface;

namespace Service.Implementation;

public class ServerService : IServerService
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IOrderRepository _detailedOrderRepository;
    private readonly IRepository<ProductInOrder> _productInOrderRepository;
    private readonly IRepository<Product> _productRepository;

    public ServerService(IRepository<Order> orderRepository, IOrderRepository detailedOrderRepository, IRepository<ProductInOrder> productInOrderRepository, IRepository<Product> productRepository)
    {
        _orderRepository = orderRepository;
        _detailedOrderRepository = detailedOrderRepository;
        _productInOrderRepository = productInOrderRepository;
        _productRepository = productRepository;
    }

    public ICollection<Order> GetPersonalizedOrdersByState(State state, String id)
    {
        return _detailedOrderRepository.GetAllOrders()
            .Where(i => i.OrderState == state)
            .Where(i => i.CreatedBy.Id == id)
            .ToList();
    }

    public Order CreateOrder(Order order, String userId)
    {
        order.Id = Guid.NewGuid();
        order.CreatedWhen = DateTime.UtcNow;
        order.Total = 0;
        order.CreatedById = userId;
        _orderRepository.Insert(order);
        return order;
    }

    public void AddProductToOrder(Order order, Product product, int quantity)
    {
        product.Quantity -= quantity;
        ProductInOrder productInOrder = new ProductInOrder
        {
            OrderId = order.Id,
            ProductId = product.Id,
            Quantity = quantity
        };
        order.Total += product.Price * productInOrder.Quantity;
        _productInOrderRepository.Insert(productInOrder);
        _orderRepository.Update(order);
    }

    public Order ChangeOrderState(Order order, State state)
    {
        order.OrderState = state;
        _orderRepository.Update(order);
        return order;
    }

    public Order GetOrderDetails(Guid? id)
    {
        return _detailedOrderRepository.GetOrderDetails(id);
    }

    public void CancelOrder(Order order)
    {
        var productsInOrder = order.ProductsInOrder;
        var doneProductsCounter = 0;
        var itemsToBeRemoved = new List<ProductInOrder>();
        
        foreach (var productInOrder in productsInOrder)
        {
            if (!productInOrder.Done)
            {
                var product = _productRepository.Get(productInOrder.ProductId);
                if (product != null)
                {
                    product.Quantity += productInOrder.Quantity;
                    _productRepository.Update(product);
                }
                order.Total -= productInOrder.Product.Price * productInOrder.Quantity; 
                // _productInOrderRepository.Delete(productInOrder);
                itemsToBeRemoved.Add(productInOrder);
            }
            else
            {
                doneProductsCounter++;
            }
        }

        foreach (var item in itemsToBeRemoved)
        {
            _productInOrderRepository.Delete(item);
        }
        if (doneProductsCounter == 0)
        {
            order.OrderState = State.CANCELLED;
        }
        else
        {
            order.OrderState = State.DELIVERED;
        }
        _orderRepository.Update(order);
    }
}