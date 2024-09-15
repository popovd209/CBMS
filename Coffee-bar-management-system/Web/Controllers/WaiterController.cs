using Entity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Repository;
using Service;
using System.Security.Claims;
using Service.Interface;
using Stripe.Checkout;
using Newtonsoft.Json;

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


    public IActionResult OrderConfirmation()
    {
        var service = new SessionService();
        Session session = service.Get(TempData["Session"].ToString());

        if (session.PaymentStatus == "paid")
        {
            return RedirectToAction(nameof(PaymentSuccess));
        }

        return RedirectToAction(nameof(PaymentFailed));
    }

    public IActionResult PaymentSuccess()
    {
        Guid orderId = Guid.Parse(TempData["OrderId"].ToString());
        var order = _waiterService.GetOrderDetails(orderId);

        _waiterService.ChangeOrderState(order, State.PAID);
        
        return View();
    }

    public IActionResult PaymentFailed()
    {
        return View();
    }

    [HttpPost, ActionName("PayInCash")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PayInCash(Guid id)
    {
        var order = _waiterService.GetOrderDetails(id);

        if (order == null)
        {
            return NotFound();
        }

        // Mark the order as paid
        _waiterService.ChangeOrderState(order, State.PAID);

        TempData["OrderId"] = order.Id;

        return RedirectToAction(nameof(PaymentSuccess));
    }

    [HttpPost, ActionName("PayWithStripe")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PayWithStripe(Guid? id)
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

        long stripeMinAmount = 5000;
        long totalAmount = 0;

        var domain = "https://cbms.azurewebsites.net/";

        var options = new SessionCreateOptions
        {
            SuccessUrl = domain + $"Waiter/OrderConfirmation",
            CancelUrl = domain + $"Waiter/CancelUrl",
            LineItems = new List<SessionLineItemOptions>(),
            Mode = "payment",
        };

        foreach (var item in order.ProductsInOrder)
        {
            long calculatedAmount = (long)(item.Product.Price * item.Quantity * 55.5);
            totalAmount += calculatedAmount;

            var sessionListItem = new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = calculatedAmount,
                    Currency = "MKD",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.Product.Name,
                        Description = item.Product.Category
                    },
                },
                Quantity = item.Quantity
            };

            options.LineItems.Add(sessionListItem);
        }

        // If the total amount is below the minimum threshold, add a service fee
        if (totalAmount < stripeMinAmount)
        {
            long serviceFeeAmount = stripeMinAmount - totalAmount;

            var serviceFeeItem = new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = serviceFeeAmount,
                    Currency = "MKD",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "Service Fee",
                        Description = "Additional service fee to meet Stripe's minimum amount"
                    },
                },
                Quantity = 1
            };

            options.LineItems.Add(serviceFeeItem);
        }

        var service = new SessionService();
        Session session = service.Create(options);

        TempData["Session"] = session.Id;
        TempData["OrderId"] = order.Id;

        Response.Headers.Append("Location", session.Url);
        return new StatusCodeResult(303);
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
        
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Index));
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
            ViewData["Order"] = order;
            ViewData["ErrorMessages"] = errorMessages;

            return View("OrderAgain");
        }
        else
        {
            foreach (var product in productsToAdd)
            {
                _waiterService.AddProductToOrder(order, product.Item1, product.Item2);
            }

            _waiterService.ChangeOrderState(order, State.NEW);
        }

        return RedirectToAction(nameof(Index));
    }
}