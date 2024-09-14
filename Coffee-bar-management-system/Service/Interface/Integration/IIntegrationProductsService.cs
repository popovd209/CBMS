using Entity.Models.Integration;

namespace Service.Interface.Integration;

public interface IIntegrationProductsService
{
    ICollection<IntegrationProduct> GetAllProducts();
}