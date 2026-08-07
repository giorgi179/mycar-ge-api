using System.Security.Claims;
using CarProject.Data;
using CarProject.Models;
using CarProject.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using WebApplication1;

namespace CarProject.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CarController : ControllerBase
{
    private readonly Base baza;

    public CarController(Base context)
    {
        baza = context;
    }

    EmailSender emailSender = new EmailSender();

    [HttpGet("get-all-car")]
    public ActionResult GetAllCar()
    {
        var getCar = baza.Cars
            .Include(c => c.CarDetals)
            .Include(c => c.Images)
            .ToList();

        return Ok(getCar);
    }

    [HttpGet("get-car-id/{id}")]
    public ActionResult GetCarId(int id)
    {
        var getCarId = baza.Cars
            .Include(c => c.CarDetals)
            .Include(c => c.Images)
            .FirstOrDefault(c => c.Id == id);

        if (getCarId == null)
            return NotFound("id Not found");

        return Ok(getCarId);
    }

    [HttpGet("get-all-car-detals")]
    public ActionResult GetAllCarDetals()
    {
        var getCarDetals = baza.CarDetals.ToList();
        return Ok(getCarDetals);
    }

    [HttpGet("get-car-search")]
    public ActionResult GetCarSearch([FromQuery] string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return BadRequest("Search term is required.");

        searchTerm = searchTerm.Trim().ToLower();

        var searchResults = baza.Cars
            .Include(c => c.CarDetals)
            .Where(c =>
                c.CarModel.ToLower().Contains(searchTerm) ||
                c.CarAge.ToLower().Contains(searchTerm) ||
                (c.CarDetals != null && c.CarDetals.UserPhone.ToLower().Contains(searchTerm)) ||
                (c.CarDetals != null && c.CarDetals.VinCode.ToLower().Contains(searchTerm))
            )
            .ToList();

        return Ok(searchResults);
    }

    [Authorize]
    [HttpGet("get-my-cars")]
    public ActionResult GetMyCars()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized("Invalid or missing user token.");

        var cars = baza.Cars
            .Include(c => c.CarDetals)
            .Include(c => c.Images)
            .Where(c => c.UserId == userId)
            .ToList();

        return Ok(cars);
    }
    [HttpGet("get-manufacturers")]
    public ActionResult GetManufacturers()
    {
        var manufacturers = baza.CarDetals
            .Where(cd => cd.Manufacturer != null)
            .GroupBy(cd => cd.Manufacturer)
            .Select(g => new { name = g.Key, count = g.Count() })
            .OrderByDescending(g => g.count)
            .Take(12)
            .ToList();

        return Ok(manufacturers);
    }

    [HttpGet("get-stats")]
    public ActionResult GetStats()
    {
        var totalCars = baza.Cars.Count();
        var totalCities = baza.Cars.Select(c => c.City).Distinct().Count();
        var totalBrands = baza.CarDetals
            .Where(cd => cd.Manufacturer != null)
            .Select(cd => cd.Manufacturer)
            .Distinct()
            .Count();

        return Ok(new
        {
            totalCars,
            totalCities,
            totalBrands
        });
    }

    [Authorize]
    [HttpPost("add-car")]
    public ActionResult AddCar([FromForm] CarRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized("Invalid or missing user token.");
        }

        var userExists = baza.Users.Any(u => u.Id == userId);
        if (!userExists)
        {
            return BadRequest("User not found.");
        }

        const int minImages = 1;
        const int maxImages = 6;

        if (request.Images == null || request.Images.Count < minImages || request.Images.Count > maxImages)
        {
            return BadRequest($"საჭიროა {minImages}-დან {maxImages}-მდე ფოტოს ატვირთვა.");
        }

        foreach (var image in request.Images)
        {
            if (image.Length > 5 * 1024 * 1024)
            {
                return BadRequest("Image max 5MB");
            }

            var ext = Path.GetExtension(image.FileName).ToLower();

            string[] allowed =
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            if (!allowed.Contains(ext))
            {
                return BadRequest("Invalid format");
            }
        }

        // Car შექმნა
        Car newCar = new Car
        {
            CarImg = "",
            City = request.City,
            CarAge = request.CarAge,
            CarModel = request.CarModel,
            CarPrice = request.CarPrice,
            CarType = request.CarType,
            FuelType = request.FuelType,
            UserId = userId
        };

        baza.Cars.Add(newCar);
        baza.SaveChanges();

        // Car Details შექმნა
        CarDetals details = new CarDetals
        {
            Manufacturer = request.Manufacturer,
            Mileage = request.Mileage,
            EngineVolume = request.EngineVolume,
            Cylinders = request.Cylinders,
            Transmission = request.Transmission,
            DriveType = request.DriveType,
            Doors = request.Doors,
            Airbags = request.Airbags,
            SteeringWheel = request.SteeringWheel,
            Color = request.Color,
            InteriorColor = request.InteriorColor,
            InteriorMaterial = request.InteriorMaterial,
            IsExchangePossible = request.IsExchangePossible,
            HasTechInspection = request.HasTechInspection,
            HasCatalyst = request.HasCatalyst,
            Description = request.Description,
            UserPhone = request.UserPhone,
            VinCode = request.VinCode,
            CarId = newCar.Id
        };

        baza.CarDetals.Add(details);

        string folder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot/images/cars"
        );

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        int index = 0;

        foreach (var image in request.Images)
        {
            string fileName =
                Guid.NewGuid().ToString()
                + Path.GetExtension(image.FileName);

            string path = Path.Combine(folder, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                image.CopyTo(stream);
            }

            string imageUrl = "/images/cars/" + fileName;

            if (index == 0)
            {
                newCar.CarImg = imageUrl;
            }

            baza.CarImages.Add(
                new CarImage
                {
                    ImageUrl = imageUrl,
                    CarId = newCar.Id
                }
            );

            index++;
        }

        baza.SaveChanges();

        return Ok(new
        {
            message = "Car added successfully",
            carId = newCar.Id
        });
    }

    [HttpDelete("car-{id}-detele")]
    public ActionResult CarIdDelete(int id)
    {
        var carId = baza.Cars.Include(c => c.CarDetals).FirstOrDefault(c => c.Id == id);
        if (carId == null)
            return NotFound("id Not Found");

        baza.Cars.Remove(carId);
        baza.SaveChanges();

        return Ok("Successful Remove");
    }



}