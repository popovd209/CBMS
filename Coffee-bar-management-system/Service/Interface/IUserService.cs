using Entity.DTOs;
using Entity.Models.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface;
public interface IUserService
{
    List<CbmsUser> GetAllWithRole(string role);

    Task<WaiterPerformanceDTO> GetWaiterPerformanceForDate(string waiterId, DateTime date);
}
