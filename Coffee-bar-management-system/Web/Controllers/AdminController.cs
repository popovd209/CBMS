using Entity.DTOs;
using Entity.Models;
using Entity.Models.Identity;
using Microsoft.AspNetCore.Mvc;
using Service.Interface;

namespace Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly IProductsService _productsService;
    private readonly IUserService _userService;

    public AdminController(IProductsService productsService, IUserService userService)
    {
        _userService = userService;
        _productsService = productsService;
    }

    [HttpGet("[action]")]
    public WaiterDTO GetUserWithId([FromQuery] string id)
    {   
        return _userService.GetUserById(id);
    }

    [HttpPost("[action]")]
    public ICollection<WaiterDTO> GetUserWithRole([FromBody] RoleModel model)
    {
        var users = _userService.GetAllWithRole(model.Role);

        var waiterDTOs = users.Select(user => new WaiterDTO
        {
            Id = user.Id,
            Name = user.FullName
        }).ToList();

        return waiterDTOs;
    }
    
    [HttpPost("[action]")]
    public WaiterPerformanceDTO GetPerformanceByWaiterAndDate([FromBody] SearchModelDTO model)
    {
        WaiterPerformanceDTO waiterPerformanceDTO = _userService.GetWaiterPerformanceForDate(model.Id, model.Date);

        return waiterPerformanceDTO;
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
