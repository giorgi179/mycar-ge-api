namespace CarProject.Models;

public class CarImage
{
    public int Id { get; set; }

    public string ImageUrl { get; set; }

    // Foreign Key
    public int CarId { get; set; }

    // Navigation
    public Car Car { get; set; }
}