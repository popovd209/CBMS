using Microsoft.AspNetCore.Identity;

namespace Entity.models.identity;

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

    public virtual ICollection<Order>? Orders { get; set; }
}
