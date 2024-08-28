using Entity.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;

namespace Repository.Implementation;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext context;
    private DbSet<CbmsUser> entities;
    string errorMessage = string.Empty;

    public UserRepository(ApplicationDbContext context)
    {
        this.context = context;
        entities = context.Set<CbmsUser>();
    }
    public IEnumerable<CbmsUser> GetAll()
    {
        return entities.AsEnumerable();
    }

    public CbmsUser Get(string id)
    {
        return entities
            .Include(z => z.Orders)
            .Include("Orders.ProductsInOrder")
            .Include("Orders.ProductsInOrder.Product")
            .SingleOrDefault(s => s.Id == id);
    }
    public void Insert(CbmsUser entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }
        entities.Add(entity);
        context.SaveChanges();
    }

    public void Update(CbmsUser entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }
        entities.Update(entity);
        context.SaveChanges();
    }

    public void Delete(CbmsUser entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }
        entities.Remove(entity);
        context.SaveChanges();
    }

}