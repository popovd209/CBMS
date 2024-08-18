using Entity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Repository;
using System.Security.Claims;

namespace Web.Controllers;

public class ServerController : Controller
{
    private readonly ApplicationDbContext _context;

    public ServerController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var completedOrders = await _context.Orders
            .Include(o => o.CreatedBy)
            .Where(o => o.OrderState == State.COMPLETE)
            .ToListAsync();
        var pendingOrders = await _context.Orders
            .Include(o => o.CreatedBy)
            .Where(o => o.OrderState == State.NEW)
            .ToListAsync();
        var deliveredOrders = await _context.Orders
            .Include(o => o.CreatedBy)
            .Where(o => o.OrderState == State.DELIVERED)
            .ToListAsync();
       
        ViewData["completedOrders"] = completedOrders;
        ViewData["deliveredOrders"] = deliveredOrders;
        ViewData["pendingOrders"] = pendingOrders;
        return View();
    }
    
    public IActionResult Create()
    {
        ViewData["Products"] = new SelectList(_context.Products, "Id", "Name");
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("TableTag,Id")] Order order, List<Guid> productIds, List<int> quantities)
    {
        if (ModelState.IsValid)
        {
            order.Id = Guid.NewGuid();
            order.CreatedWhen = DateTime.UtcNow;
            order.Total = 0;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            order.CreatedById = userId;

            for (int i = 0; i < productIds.Count; i++)
            {
                ProductInOrder productInOrder = new ProductInOrder
                {
                    OrderId = order.Id,
                    ProductId = productIds[i],
                    Quantity = quantities[i]
                };

                Product? product = await _context.Products.FindAsync(productIds[i]);
                if (product != null)
                {
                    order.Total += product.Price * productInOrder.Quantity;
                }

                _context.ProductsInOrder.Add(productInOrder);
            }

            _context.Add(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["Products"] = new SelectList(_context.Products, "Id", "Name");
        return View(order);
    }
    
    [HttpPost, ActionName("Deliver")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeliverOrder(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        
        var order = await _context.Orders
            .FirstOrDefaultAsync(m => m.Id == id);
        
        if (order == null)
        {
            return NotFound();
        }
        
        order.OrderState = State.DELIVERED;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost, ActionName("Pay")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PayOrder(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        
        var order = await _context.Orders
            .FirstOrDefaultAsync(m => m.Id == id);
        
        if (order == null)
        {
            return NotFound();
        }
        
        order.OrderState = State.PAID;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost, ActionName("Cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelOrder(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        
        var order = await _context.Orders
            .FirstOrDefaultAsync(m => m.Id == id);
        
        if (order == null)
        {
            return NotFound();
        }
        
        order.OrderState = State.CANCELLED;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> OrderAgain(Guid? id)
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
        ViewData["Products"] = new SelectList(_context.Products, "Id", "Name");
        ViewData["Order"] = order;
        
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reorder(Guid id, List<Guid> productIds, List<int> quantities)
    {
        if (id == null)
        {
            return NotFound();
        }
        
        var order = await _context.Orders
            .FirstOrDefaultAsync(m => m.Id == id);
        
        if (order == null)
        {
            return NotFound();
        }
        
        if (ModelState.IsValid)
        {
            for (int i = 0; i < productIds.Count; i++)
            {
                ProductInOrder productInOrder = new ProductInOrder
                {
                    OrderId = id,
                    ProductId = productIds[i],
                    Quantity = quantities[i]
                };

                Product? product = await _context.Products.FindAsync(productIds[i]);
                if (product != null)
                {
                    order.Total += product.Price * productInOrder.Quantity;
                }

                _context.ProductsInOrder.Add(productInOrder);
            }

            order.OrderState = State.NEW;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        return RedirectToAction(nameof(Index));
    }
}