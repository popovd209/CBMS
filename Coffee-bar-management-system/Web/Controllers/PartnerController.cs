using Microsoft.AspNetCore.Mvc;
using Service.Interface.Integration;

namespace Web.Controllers;

public class PartnerController : Controller
{
    private readonly IIntegrationProductsService _productsService;
    private readonly IIntegrationCategoriesService _categoriesService;

    public PartnerController(IIntegrationProductsService productsService, IIntegrationCategoriesService categoriesService)
    {
        _productsService = productsService;
        _categoriesService = categoriesService;
    }
    
    public async Task<IActionResult> Index()
    {
        var products = _productsService.GetAllProducts();
        var categories = _categoriesService.GetAllCategories();
       
        ViewData["products"] = products;
        ViewData["categories"] = categories;
        
        return View();
    }
}