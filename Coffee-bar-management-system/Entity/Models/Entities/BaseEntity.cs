using System.ComponentModel.DataAnnotations;

namespace Entity.Models;

public class BaseEntity
{
    [Key]
    public Guid Id { get; set; }
}
