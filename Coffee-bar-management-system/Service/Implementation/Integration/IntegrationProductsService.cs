using Entity.Models.Integration;
using Repository.Interface.Integration;
using Service.Interface.Integration;

namespace Service.Implementation.Integration;

public class IntegrationProductsService : IIntegrationProductsService
{
    private readonly IIntegrationRepository<IntegrationProduct> _productsRepository;

    public IntegrationProductsService(IIntegrationRepository<IntegrationProduct> productsRepository)
    {
        _productsRepository = productsRepository;
    }
    
    public ICollection<IntegrationProduct> GetAllProducts()
    {
        return _productsRepository.GetAll().ToList();
    }
}