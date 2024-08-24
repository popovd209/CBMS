using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Service
{
    public class SeedData
    {
        public enum Role
        {
            Admin,
            Bartender,
            Waiter
        }

        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (Role role in Enum.GetValues(typeof(Role)))
            {
                string roleName = role.ToString();

                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        public static List<string> getRoles()
        {
            return Enum.GetValues(typeof(Role))
               .Cast<Role>()
               .Select(role => role.ToString())
               .ToList();
        }


        public static class GetRoleFor
        {
            public const string Admin = "Admin";
            public const string Bartender = "Bartender";
            public const string Waiter = "Waiter";
        }
    }
}
