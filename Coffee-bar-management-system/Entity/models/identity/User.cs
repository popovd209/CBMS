using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Entity.models.identity
{
    public class User : IdentityUser
    {
        public string FullName { get; set; }
        public int PersonalPin { get; set; }
        public enum Position
        {
            WAITER,
            BARTENDER
        }
        public DateOnly Date { get; set; }
        public ICollection<Order>? Orders { get; set; }
    }
}
