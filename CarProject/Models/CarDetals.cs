namespace CarProject.Models;

public class CarDetals
{
    public int Id { get; set; }

    public string Manufacturer { get; set; }      // მწარმოებელი (Alfa Romeo)
    public string Mileage { get; set; }           // გარბენი (68000 km)
    public string EngineVolume { get; set; }      // ძრავის მოცულობა (2.9 turbo)
    public int Cylinders { get; set; }            // ცილინდრები (4)
    public string Transmission { get; set; }      // გადაცემათა კოლოფი (ტიპტრონიკი)
    public string DriveType { get; set; }         // წამყვანი თვლები (4x4)
    public string Doors { get; set; }             // კარები (4/5)
    public int Airbags { get; set; }              // აირბეგი (10)
    public string SteeringWheel { get; set; }     // საჭე (მარცხენა)
    public string Color { get; set; }             // ფერი (შავი)
    public string InteriorColor { get; set; }     // სალონის ფერი (შავი)
    public string InteriorMaterial { get; set; }  // სალონის მასალა (ალკანტარა)
    public bool IsExchangePossible { get; set; }  // გაცვლა (არა/კი)
    public bool HasTechInspection { get; set; }   // ტექ. დათვალიერება (კი/არა)
    public bool HasCatalyst { get; set; }         // კატალიზატორი (კი/არა)

    public string Description { get; set; }       // დეტალური აღწერა (თავისუფალი ტექსტი)
    public string UserPhone { get; set; }
    public string VinCode { get; set; }          // მანქანის ვინ კოდი (VIN)

    public int CarId { get; set; }                 // Foreign key Car-ისთვის (one-to-one)
    public Car Car { get; set; }                  // Navigation property Car-ისკენ (one-to-one)
}