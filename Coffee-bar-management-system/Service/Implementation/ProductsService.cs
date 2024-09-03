using Entity.DTOs;
using Entity.Models;
using Repository.Interface;
using Service.Interface;

namespace Service.Implementation;

public class ProductsService : IProductsService
{
    private readonly IRepository<Product> _productsRepository;

    public ProductsService(IRepository<Product> productsRepository)
    {
        _productsRepository = productsRepository;
    }


    public ICollection<Product> GetAllProducts()
    {
        return _productsRepository.GetAll().ToList();
    }

    public Product GetProductDetails(Guid? id)
    {
        return _productsRepository.Get(id);
    }

    public Product CreateAProduct(Product product)
    {
        product.Id = Guid.NewGuid();
        _productsRepository.Insert(product);
        return product;
    }

    public Product UpdateProduct(Product product)
    {
        _productsRepository.Update(product);
        return product;
    }

    public void DeleteProduct(Product product)
    {
        _productsRepository.Delete(product);
    }

    public Product AddProductStorage(Product product, int quantity)
    {
        product.Quantity += quantity;
        _productsRepository.Update(product);
        return product;
    }

    public Product? CheckIfExists(ImportProductDTO product)
    {
        return _productsRepository.GetAll()
            .FirstOrDefault(x => x.Name == product.Name && x.Category == product.Category);
    }
}