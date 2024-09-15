using Entity.Models;

namespace Repository.Interface.Integration;

public interface IIntegrationRepository<T> where T : BaseEntity
{
    IEnumerable<T> GetAll();
}