using Entity.Models.Integration;
using Repository.Interface.Integration;
using Service.Interface.Integration;

namespace Service.Implementation.Integration;

public class IntegrationCategoriesService : IIntegrationCategoriesService
{
    private readonly IIntegrationRepository<IntegrationCategory> _categoriesRepository;

    public IntegrationCategoriesService(IIntegrationRepository<IntegrationCategory> categoriesRepository)
    {
        _categoriesRepository = categoriesRepository;
    }
    
    public ICollection<IntegrationCategory> GetAllCategories()
    {
        return _categoriesRepository.GetAll().ToList();
    }
}