using Entity.models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Controllers;

public class BartenderController : Controller
{
    private readonly ApplicationDbContext _context;

    public BartenderController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var pendingOrders = await _context.Orders
            .Where(o => o.OrderState == State.NEW)
            .ToListAsync();
        
        return View(pendingOrders);
    }
    
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var order = await _context.Orders
            .Include(o => o.CreatedBy)
            .Include(o => o.ProductsInOrder)
            .Include("ProductsInOrder.Product")
            .FirstOrDefaultAsync(m => m.Id == id);
        if (order == null)
        {
            return NotFound();
        }

        order.OrderState = State.IN_PROGRESS;
        await _context.SaveChangesAsync();

        return View(order);
    }
}