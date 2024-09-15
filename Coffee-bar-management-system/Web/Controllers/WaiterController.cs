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

public class WaiterController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWaiterService _waiterService;
    private readonly IProductsService _productsService;

    public WaiterController(ApplicationDbContext context, IWaiterService waiterService, IProductsService productsService)
    {
        _context = context;
        _waiterService = waiterService;
        _productsService = productsService;
    }

    [Authorize(Roles = SeedData.GetRoleFor.Waiter)]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var completedOrders = _waiterService.GetPersonalizedOrdersByState(State.COMPLETE, userId);
        var pendingOrders = _waiterService.GetPersonalizedOrdersByState(State.NEW, userId);
        var deliveredOrders = _waiterService.GetPersonalizedOrdersByState(State.DELIVERED, userId);
       
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
        if (!ModelState.IsValid)
        {
            var products = _productsService.GetAllProducts();
            ViewData["Products"] = new SelectList(products, "Id", "Name");
            return View(order);
        }

        bool hasError = false;
        List<string> errorMessages = [];
        List<Tuple<Product, int>> productsToAdd = new List<Tuple<Product, int>>();

        for (int i = 0; i < productIds.Count; i++)
        {
            Product? product = _productsService.GetProductDetails(productIds[i]);
            if (product == null)
            {
                break;
            }

            var availableQuantity = product.Quantity;
            var seekedQuantity = quantities[i];

            if (seekedQuantity > availableQuantity)
            {
                errorMessages.Add($"Not enough {product.Name} in storage. You selected {seekedQuantity}, there is only {availableQuantity} in storage.");
                hasError = true;
            }
            else
            {
                productsToAdd.Add(new Tuple<Product, int>(product, seekedQuantity));
            }
        }

        if (hasError)
        {
            var products = _productsService.GetAllProducts();
            ViewData["Products"] = new SelectList(products, "Id", "Name");
            ViewData["ErrorMessages"] = errorMessages;
            return View(order);
        }
        else
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var newOrder = _waiterService.CreateOrder(order, userId);

            foreach (var product in productsToAdd)
            {
                _waiterService.AddProductToOrder(newOrder, product.Item1, product.Item2);
            }
        }

        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost, ActionName("Deliver")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeliverOrder(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var order = _waiterService.GetOrderDetails(id);
        
        if (order == null)
        {
            return NotFound();
        }

        _waiterService.ChangeOrderState(order, State.DELIVERED);

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

        var order = _waiterService.GetOrderDetails(id);
        
        if (order == null)
        {
            return NotFound();
        }

        _waiterService.ChangeOrderState(order, State.PAID);

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

        var order = _waiterService.GetOrderDetails(id);
        
        if (order == null)
        {
            return NotFound();
        }
        _waiterService.CancelOrder(order);
        
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> OrderAgain(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var order = _waiterService.GetOrderDetails(id);
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
        var order = _waiterService.GetOrderDetails(id);
        
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
                        _waiterService.AddProductToOrder(order, product, seekedQuantity);
                    }
                    else
                    {
                        return View("NotEnoughProducts", product);
                    }
                }
            }

            _waiterService.ChangeOrderState(order, State.NEW);
            return RedirectToAction(nameof(Index));
        }
        
        return RedirectToAction(nameof(Index));
    }
}