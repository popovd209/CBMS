using Entity.DTOs;
using Entity.Models;
using Microsoft.AspNetCore.Mvc;
using Service.Interface;

namespace Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AdminController : Controller
{
    private readonly IProductsService _productsService;

    public AdminController(IProductsService productsService)
    {
        _productsService = productsService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("[action]")]
    public ICollection<Product> GetAllProducts()
    {
        return _productsService.GetAllProducts();
    }

    [HttpPost("[action]")]
    public bool ImportProducts(List<ImportProductDTO> model)
    {
        bool status = true;

        foreach (ImportProductDTO item in model)
        {
            Product productCheck = _productsService.CheckIfExists(item);

            if (productCheck == null)
            {
                Product product = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = item.Name,
                    Price = item.Price,
                    Category = item.Category,
                    Quantity = item.Quantity
                };

                _productsService.CreateAProduct(product);
            }
            else
            {
                productCheck.Quantity += item.Quantity;
                productCheck.Price = item.Price;

                _productsService.UpdateProduct(productCheck);
            }
        }
        return status;
    }
}
