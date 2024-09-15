using Entity.Models.Integration;
using Microsoft.EntityFrameworkCore;

namespace Repository;

public class IntegrationDbContext : DbContext
{
    public IntegrationDbContext(DbContextOptions<IntegrationDbContext> options)
        : base(options)
    {
        
    }
    
    public virtual DbSet<IntegrationCategory> Categories { get; }
    public virtual DbSet<IntegrationProduct> Products { get; }
}