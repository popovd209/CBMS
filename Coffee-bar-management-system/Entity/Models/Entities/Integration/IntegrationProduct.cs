using System.ComponentModel.DataAnnotations;

namespace Entity.Models.Integration;

public class IntegrationProduct : BaseEntity
{
    public Guid Id { get; set; }
    public string ProductName { get; set; }
    public int Price { get; set; }
    public int Rating { get; set; }
    
    public string Name
    {
        get
        {
            return ProductName.Replace(" ", "-");
        }
    }

    public double PriceInDenars
    {
        get
        {
            return Price * 61.5;
        }
    }

    public int RatingOutOfTen
    {
        get
        {
            return Rating * 2;
        }
    }
}