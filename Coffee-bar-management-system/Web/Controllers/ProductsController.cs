using Microsoft.AspNetCore.Mvc;
using Entity.Models;
using Service.Interface;

namespace Web.Controllers
{

    public class ProductsController : Controller
    {
        private readonly IProductsService _productsService;

        public ProductsController(IProductsService productsService)
        {
            _productsService = productsService;
        }


        public async Task<IActionResult> Index()
        {
            var products = _productsService.GetAllProducts();
            return View(products);
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Product? product = _productsService.GetProductDetails(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Price,Category,Id")] Product product)
        {
            if (ModelState.IsValid)
            {
                _productsService.CreateAProduct(product);
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Product? product = _productsService.GetProductDetails(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Name,Price,Category,Quantity,Id")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _productsService.UpdateProduct(product);
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = _productsService.GetProductDetails(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var product = _productsService.GetProductDetails(id);
            _productsService.DeleteProduct(product);
            return RedirectToAction(nameof(Index));
        }

        public async Task<ActionResult> AddToStorage(Guid id)
        {
            Product? product = _productsService.GetProductDetails(id);
            return View(product);
        }

        [HttpPost, ActionName("AddToStorage")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddToStorage(Guid id, int quantity)
        {
            Product? product = _productsService.GetProductDetails(id);

            if (product == null)
            {
                return NotFound();
            }

            if (quantity < 0)
            {
                ViewData["errorMessage"] = "Quantity cannot be a negative number.";
                return View("AddToStorage", product);
            }

            _productsService.AddProductStorage(product, quantity);
            return RedirectToAction("Index");
        }
    }
}