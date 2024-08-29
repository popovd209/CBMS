using Entity.Models;

namespace Service.Interface;

public interface IProductsService
{
    ICollection<Product> GetAllProducts();
    Product GetProductDetails(Guid? id);
    Product CreateAProduct(Product product);
    Product UpdateProduct(Product product);
    void DeleteProduct(Product product);
    Product AddProductStorage(Product product, int quantity);
}