using System.ComponentModel.DataAnnotations;

namespace Admin_CBMS.Models
{
    public class BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
    }
}