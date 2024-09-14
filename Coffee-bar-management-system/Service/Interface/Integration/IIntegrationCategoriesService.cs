using Entity.Models.Integration;

namespace Service.Interface.Integration;

public interface IIntegrationCategoriesService
{
    ICollection<IntegrationCategory> GetAllCategories();
}