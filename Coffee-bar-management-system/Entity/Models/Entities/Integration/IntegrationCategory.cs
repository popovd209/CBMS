using System.ComponentModel.DataAnnotations;

namespace Entity.Models.Integration;

public class IntegrationCategory : BaseEntity
{
    public Guid Id { get; set; }
    public string CategoryName { get; set; }

    public string Name
    {
        get
        {
            return CategoryName.Replace(" ", "-");
        }
    }
}