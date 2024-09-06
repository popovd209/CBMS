using Entity.Models.Identity;

namespace Repository.Interface;

public interface IUserRepository
{
    IEnumerable<CbmsUser> GetAll();
    List<CbmsUser> GetAllWithRoleAsync(string role);
    CbmsUser Get(string? id);
    void Insert(CbmsUser entity);
    void Update(CbmsUser entity);
    void Delete(CbmsUser entity);
}