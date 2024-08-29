using Entity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Repository;
using Service;
using System.Security.Claims;
using Service.Interface;

namespace Web.Controllers;

public class ServerController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IServerService _serverService;
    private readonly IProductsService _productsService;

    public ServerController(ApplicationDbContext context, IServerService serverService, IProductsService productsService)
    {
        _context = context;
        _serverService = serverService;
        _productsService = productsService;
    }

    [Authorize(Roles = SeedData.GetRoleFor.Waiter)]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var completedOrders = _serverService.GetPersonalizedOrdersByState(State.COMPLETE, userId);
        var pendingOrders = _serverService.GetPersonalizedOrdersByState(State.NEW, userId);
        var deliveredOrders = _serverService.GetPersonalizedOrdersByState(State.DELIVERED, userId);
       
        ViewData["completedOrders"] = completedOrders;
        ViewData["deliveredOrders"] = deliveredOrders;
        ViewData["pendingOrders"] = pendingOrders;
        return View();
    }
    
    public IActionResult Create()
    {
        var products = _productsService.GetAllProducts();
        ViewData["Products"] = new SelectList(products, "Id", "Name");
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("TableTag,Id")] Order order, List<Guid> productIds, List<int> quantities)
    {
        if (ModelState.IsValid)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var newOrder = _serverService.CreateOrder(order, userId);
            
            for (int i = 0; i < productIds.Count; i++)
            {
                Product? product = _productsService.GetProductDetails(productIds[i]);
                
                if (product != null)
                {
                    var availableQuantity = product.Quantity;
                    var seekedQuantity = quantities[i];
                    if (quantities[i] < 0)
                    {
                        ViewData["errorMessage"] = "Quantity cannot be a negative number.";
                        return View("Create", newOrder);
                    }
                    
                    if (seekedQuantity <= availableQuantity)
                    {
                        _serverService.AddProductToOrder(newOrder, product, seekedQuantity);
                    }
                    else
                    {
                        return View("NotEnoughProducts", product);
                    }
                }
            }
            return RedirectToAction(nameof(Index));
        }

        var products = _productsService.GetAllProducts();
        ViewData["Products"] = new SelectList(products, "Id", "Name");
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

        var order = _serverService.GetOrderDetails(id);
        
        if (order == null)
        {
            return NotFound();
        }

        _serverService.ChangeOrderState(order, State.DELIVERED);

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

        var order = _serverService.GetOrderDetails(id);
        
        if (order == null)
        {
            return NotFound();
        }

        _serverService.ChangeOrderState(order, State.PAID);

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

        var order = _serverService.GetOrderDetails(id);
        
        if (order == null)
        {
            return NotFound();
        }
        _serverService.CancelOrder(order);
        
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> OrderAgain(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var order = _serverService.GetOrderDetails(id);
        if (order == null)
        {
            return NotFound();
        }

        var products = _productsService.GetAllProducts();
        ViewData["Products"] = new SelectList(products, "Id", "Name");
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

        var order = _serverService.GetOrderDetails(id);
        
        if (order == null)
        {
            return NotFound();
        }
        
        if (ModelState.IsValid)
        {
            for (int i = 0; i < productIds.Count; i++)
            {
                Product? product = _productsService.GetProductDetails(productIds[i]);
                
                if (product != null)
                {
                    var availableQuantity = product.Quantity;
                    var seekedQuantity = quantities[i];
                    if (quantities[i] < 0)
                    {
                        ViewData["errorMessage"] = "Quantity cannot be a negative number.";
                        return View("OrderAgain", order);
                    }
                    if (seekedQuantity <= availableQuantity)
                    {
                        _serverService.AddProductToOrder(order, product, seekedQuantity);
                    }
                    else
                    {
                        return View("NotEnoughProducts", product);
                    }
                }
            }

            _serverService.ChangeOrderState(order, State.NEW);
            return RedirectToAction(nameof(Index));
        }
        
        return RedirectToAction(nameof(Index));
    }
}