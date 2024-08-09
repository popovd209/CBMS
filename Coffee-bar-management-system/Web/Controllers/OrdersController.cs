using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Entity.models;
using Web.Data;

namespace Web.Controllers;

public class OrdersController : Controller
{
    private readonly ApplicationDbContext _context;

    public OrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var applicationDbContext = _context.Orders.Include(o => o.CreatedBy);
        return View(await applicationDbContext.ToListAsync());
    }

    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var order = await _context.Orders
            .Include(o => o.CreatedBy)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (order == null)
        {
            return NotFound();
        }

        return View(order);
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
            order.CreatedById = "713401aa-fe2b-43a3-8fb1-78cf0481df3f";

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

    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }
        ViewData["CreatedById"] = new SelectList(_context.Users, "Id", "Id", order.CreatedById);
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, [Bind("Total,TableTag,CreatedById,CreatedWhen,Id")] Order order)
    {
        if (id != order.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(order);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(order.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        ViewData["CreatedById"] = new SelectList(_context.Users, "Id", "Id", order.CreatedById);
        return View(order);
    }

    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var order = await _context.Orders
            .Include(o => o.CreatedBy)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order != null)
        {
            _context.Orders.Remove(order);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool OrderExists(Guid id)
    {
        return _context.Orders.Any(e => e.Id == id);
    }
}
