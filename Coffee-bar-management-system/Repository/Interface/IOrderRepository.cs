using System.Collections;
using Entity.Models;

namespace Repository.Interface;

public interface IOrderRepository
{
    IEnumerable<Order> GetAllOrders();
    Order GetOrderDetails(Guid? id);
}