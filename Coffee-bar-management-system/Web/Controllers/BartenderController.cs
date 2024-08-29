using Entity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository;
using Service;
using Service.Interface;


namespace Web.Controllers;

public class BartenderController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IBartenderService _bartenderService;
    
    public BartenderController(ApplicationDbContext context, IBartenderService bartenderService)
    {
        _context = context;
        _bartenderService = bartenderService;
    }

    [Authorize(Roles = SeedData.GetRoleFor.Bartender)]
    public async Task<IActionResult> Index()
    {
        var pendingOrders = _bartenderService.GetFilteredOrdersByState(State.NEW);
        var startedOrders = _bartenderService.GetFilteredOrdersByState(State.IN_PROGRESS);
       
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

        var order = _bartenderService.GetOrderById(id);
        
        if (order == null)
        {
            return NotFound();
        }

        _bartenderService.ChangeOrderStatus(order, State.IN_PROGRESS);

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
        
        var order = _bartenderService.GetOrderById(id);
        
        if (order == null)
        {
            return NotFound();
        }

        _bartenderService.MakeOrder(order);

        return RedirectToAction(nameof(Index));
    }
}