using Entity.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace Entity.DTOs
{
    public class CbmsUserRegisterDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public int PersonalPin { get; set; }

        [Required]
        public CbmsUser.Position UserPosition { get; set; }

        [DataType(DataType.Date)]
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    }
}
