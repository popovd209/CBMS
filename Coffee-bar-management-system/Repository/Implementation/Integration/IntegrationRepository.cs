using Entity.Models;
using Microsoft.EntityFrameworkCore;
using Repository.Interface.Integration;

namespace Repository.Implementation.Integration;

public class IntegrationRepository<T> : IIntegrationRepository<T> where T : BaseEntity
{
    private readonly IntegrationDbContext context;
    private DbSet<T> entities;
    string errorMessage = string.Empty;

    public IntegrationRepository(IntegrationDbContext context)
    {
        this.context = context;
        entities = context.Set<T>();
    }
    public IEnumerable<T> GetAll()
    {
        return entities.AsEnumerable();
    }
}