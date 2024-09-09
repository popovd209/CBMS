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

        public WaiterDTO GetUserById(string id)
        {
            var user = _userRepository.Get(id);

            WaiterDTO userDTO = new WaiterDTO
            {
                Id = id,
                Name = user.FullName,
                Email = user.Email,
                PersonalPin = user.PersonalPin.ToString(),
                ContractDate = user.Date.ToDateTime(TimeOnly.MinValue),
            };

            return userDTO;
        }

        public WaiterPerformanceDTO GetWaiterPerformanceForDate(string waiterId, DateTime date)
        {
            var userEntity = _userRepository.Get(waiterId);

            var result = new WaiterPerformanceDTO();

            if(userEntity != null)
            {
                var paidOrders = userEntity.Orders
                    .Where(order => order.OrderState == Entity.Models.State.PAID && order.CreatedWhen.Date.Equals(date.Date));

                var mostCommonCategory = paidOrders
                    .SelectMany(order => order.ProductsInOrder)
                    .GroupBy(productInOrder => productInOrder.Product.Category)
                    .OrderByDescending(group => group.Count())
                    .FirstOrDefault()?.Key;

                result = new WaiterPerformanceDTO
                {
                    WaiterName = userEntity.FullName,
                    TotalOrdersServed = paidOrders.Count(),
                    MostCommonCategory = mostCommonCategory,
                    TotalIncome = paidOrders.Sum(order => order.Total),
                    Date = date,
                };
            }

            return result;
        }
    }
}
