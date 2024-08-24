using Entity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository;
using Service;


namespace Web.Controllers;

public class BartenderController : Controller
{
    private readonly ApplicationDbContext _context;

    public BartenderController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = SeedData.GetRoleFor.Bartender)]
    public async Task<IActionResult> Index()
    {
        var pendingOrders = await _context.Orders
            .Include(o => o.CreatedBy)
            .Where(o => o.OrderState == State.NEW)
            .ToListAsync();
        var startedOrders = await _context.Orders
            .Include(o => o.CreatedBy)
            .Where(o => o.OrderState == State.IN_PROGRESS)
            .ToListAsync();
       
        ViewData["pendingOrders"] = pendingOrders;
        ViewData["startedOrders"] = startedOrders;
        
        return View();
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

    [HttpPost, ActionName("Finish")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FinishOrder(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        
        var order = await _context.Orders
            .Include(o=>o.ProductsInOrder)
            .FirstOrDefaultAsync(m => m.Id == id);
        
        if (order == null)
        {
            return NotFound();
        }

        var productsInOrder = order.ProductsInOrder;
        foreach (var product in productsInOrder)
        {
            product.Done = true;
        }
        
        order.OrderState = State.COMPLETE;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}