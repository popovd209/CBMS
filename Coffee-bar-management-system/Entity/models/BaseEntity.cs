using System.ComponentModel.DataAnnotations;

namespace Entity.models;

public class BaseEntity
{
    [Key]
    public Guid Id { get; set; }
}
