using Entity.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Entity.Models.Identity;

public class CbmsUser : IdentityUser
{
    [Required]
    public required string FullName { get; set; }

    [Required]
    public int PersonalPin { get; set; }

    public enum Position
    {
        WAITER,
        BARTENDER
    }

    [AllowNull]
    public DateOnly Date { get; set; }

    public virtual ICollection<Order>? Orders { get; set; }
}
