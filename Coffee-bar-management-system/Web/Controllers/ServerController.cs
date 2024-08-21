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
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var completedOrders = await _context.Orders
            .Include(o => o.CreatedBy)
            .Where(o => o.OrderState == State.COMPLETE)
            .Where(o => o.CreatedBy.Id == userId )
            .ToListAsync();
        var pendingOrders = await _context.Orders
            .Include(o => o.CreatedBy)
            .Where(o => o.OrderState == State.NEW)
            .Where(o => o.CreatedBy.Id == userId )
            .ToListAsync();
        var deliveredOrders = await _context.Orders
            .Include(o => o.CreatedBy)
            .Where(o => o.OrderState == State.DELIVERED)
            .Where(o => o.CreatedBy.Id == userId )
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
            // Take from current user
            //order.CreatedById = "713401aa-fe2b-43a3-8fb1-78cf0481df3f";

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            order.CreatedById = userId;

            for (int i = 0; i < productIds.Count; i++)
            {
                Product? product = await _context.Products.FindAsync(productIds[i]);
                if (product != null)
                {
                    var availableQuantity = product.Quantity;
                    var seekedQuantity = quantities[i];
                    if (seekedQuantity <= availableQuantity)
                    {
                        product.Quantity -= seekedQuantity;
                        ProductInOrder productInOrder = new ProductInOrder
                        {
                            OrderId = order.Id,
                            ProductId = product.Id,
                            Quantity = seekedQuantity
                        };
                        order.Total += product.Price * productInOrder.Quantity;
                        _context.ProductsInOrder.Add(productInOrder);
                    }
                    else
                    {
                        return View("NotEnoughProducts", product);
                    }
                }
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
            .Include(o => o.ProductsInOrder)
            .Include("ProductsInOrder.Product")
            .FirstOrDefaultAsync(m => m.Id == id);
        
        if (order == null)
        {
            return NotFound();
        }
        
        var productsInOrder = order.ProductsInOrder;
        var doneProductsCounter = 0;
        foreach (var product in productsInOrder)
        {
            if (!product.Done)
            {
                var productObj = await _context.Products
                    .FirstOrDefaultAsync(m => m.Id == product.ProductId);
                if (productObj != null)
                {
                    productObj.Quantity += product.Quantity;
                }
                order.Total -= product.Product.Price * product.Quantity;
                _context.ProductsInOrder.Remove(product);
            }
            else
            {
                doneProductsCounter++;
            }
        }

        if (doneProductsCounter == 0)
        {
            order.OrderState = State.CANCELLED;
        }
        else
        {
            order.OrderState = State.DELIVERED;
        }
        
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
                Product? product = await _context.Products.FindAsync(productIds[i]);
                
                if (product != null)
                {
                    var availableQuantity = product.Quantity;
                    var seekedQuantity = quantities[i];
                    if (seekedQuantity <= availableQuantity)
                    {
                        product.Quantity -= seekedQuantity;
                        ProductInOrder productInOrder = new ProductInOrder
                        {
                            OrderId = id,
                            ProductId = productIds[i],
                            Quantity = quantities[i]
                        };
                        order.Total += product.Price * productInOrder.Quantity;
                        _context.ProductsInOrder.Add(productInOrder);
                    }
                    else
                    {
                        return View("NotEnoughProducts", product);
                    }
                }
            }

            order.OrderState = State.NEW;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        return RedirectToAction(nameof(Index));
    }
}