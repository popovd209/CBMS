using Entity.DTOs;
using Entity.Models.Identity;
using Repository.Interface;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public List<CbmsUser> GetAllWithRole(string role)
        {
            return _userRepository.GetAllWithRoleAsync(role);
        }

        public Task<WaiterPerformanceDTO> GetWaiterPerformanceForDate(string waiterId, DateTime date)
        {
            throw new NotImplementedException();
        }
    }
}
