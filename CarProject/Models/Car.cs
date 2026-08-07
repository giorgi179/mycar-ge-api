namespace CarProject.Models;

public class Car
{
    public int Id { get; set; }

    public string CarImg { get; set; }

    public string City { get; set; }

    public string CarAge { get; set; }

    public string CarModel { get; set; }

    public decimal CarPrice { get; set; }

    public string CarType { get; set; }

    public string FuelType { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public CarDetals CarDetals { get; set; }

    public List<CarImage> Images { get; set; } = new();
}