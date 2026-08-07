using Microsoft.AspNetCore.Http;

namespace CarProject.Request;

public class CarRequest
{
    public string City { get; set; }
    public string CarAge { get; set; }
    public string CarModel { get; set; }
    public decimal CarPrice { get; set; }
    public string CarType { get; set; }
    public string FuelType { get; set; }

    public string Manufacturer { get; set; }
    public string Mileage { get; set; }
    public string EngineVolume { get; set; }
    public int Cylinders { get; set; }
    public string Transmission { get; set; }
    public string DriveType { get; set; }
    public string Doors { get; set; }
    public int Airbags { get; set; }
    public string SteeringWheel { get; set; }
    public string Color { get; set; }
    public string InteriorColor { get; set; }
    public string InteriorMaterial { get; set; }

    public bool IsExchangePossible { get; set; }
    public bool HasTechInspection { get; set; }
    public bool HasCatalyst { get; set; }

    public string Description { get; set; }
    public string UserPhone { get; set; }
    public string VinCode { get; set; }

    public List<IFormFile> Images { get; set; } = new();
}