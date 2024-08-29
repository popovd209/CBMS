using System.Collections;
using Entity.Models;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;

namespace Repository.Implementation;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<Order> _entities;
    
    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
        _entities = context.Set<Order>();
    }

    public IEnumerable<Order> GetAllOrders()
    {
        return _entities.Include(z => z.CreatedBy)
            .Include(z => z.ProductsInOrder)
            .Include("ProductsInOrder.Product");
    }

    public Order GetOrderDetails(Guid? id)
    {
        return _entities.Include(z => z.CreatedBy)
            .Include(z => z.ProductsInOrder)
            .Include("ProductsInOrder.Product")
            .SingleOrDefault(z => z.Id == id);
    }
}