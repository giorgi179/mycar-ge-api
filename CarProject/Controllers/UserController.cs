using System.Security.Claims;
using CarProject.Data;
using CarProject.Models;
using CarProject.Request;
using CarProject.Response;
using CarProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication1;

namespace CarProject.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{

    private readonly Base baza;
    private readonly ITokenService tokenService;
    private readonly IWebHostEnvironment env;
    private readonly EmailSender emailSender = new EmailSender();

    public UserController(Base context, ITokenService tokenService, IWebHostEnvironment env)
    {
        this.baza = context;
        this.tokenService = tokenService;
        this.env = env;
    }

    [HttpGet("get-all-users")]
    public ActionResult GetAllUsers()
    {
        var getUsers = baza.Users.ToList();
        return Ok(getUsers);
    }
    [Authorize]
    [HttpGet("get-current-user")]
    public ActionResult GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized("Invalid or missing user token.");

        var user = baza.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return NotFound("User not found.");

        return Ok(new
        {
            id = user.Id,
            firstName = user.FirstName,
            lastName = user.LastName,
            email = user.Email,
            userUrl = user.UserUrl,
            isVerified = user.IsVerified
        });
    }

    [HttpPost("register-user")]
    public async Task<ActionResult> RegisterUser([FromForm] UserRequest request)
    {
        if (request.Password != request.ReEnterPassword)
        {
            return BadRequest("Passwords do not match.");
        }

        var existingUser = baza.Users.FirstOrDefault(u => u.Email == request.Email);
        if (existingUser != null)
        {
            return BadRequest("Email already exists.");
        }

        string? userUrl = null;

        if (request.UserPhoto != null && request.UserPhoto.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(request.UserPhoto.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest("Only image files are allowed (jpg, jpeg, png, gif).");
            }

            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (request.UserPhoto.Length > maxFileSize)
            {
                return BadRequest("File size must not exceed 5MB.");
            }

            var uploadsFolder = Path.Combine(env.WebRootPath ?? "wwwroot", "uploads", "users");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.UserPhoto.CopyToAsync(stream);
            }

            userUrl = $"/uploads/users/{uniqueFileName}";
        }

        Random random = new Random();
        string verificationCode = random.Next(100000, 999999).ToString();

        User user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            UserUrl = userUrl,
            VerificationCode = verificationCode,
            VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(10),
            IsVerified = false,
            
        };

        baza.Users.Add(user);
        baza.SaveChanges();

        string emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f4f4f7; margin: 0; padding: 0; }}
        .container {{ max-width: 480px; margin: 40px auto; background: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.08); }}
        .header {{ background-color: #1a1a2e; padding: 24px; text-align: center; }}
        .header h1 {{ color: #ffffff; margin: 0; font-size: 20px; }}
        .body {{ padding: 32px; text-align: center; }}
        .body p {{ color: #555555; font-size: 15px; line-height: 1.6; }}
        .code-box {{ display: inline-block; background-color: #f0f0f5; border: 1px dashed #cccccc; border-radius: 6px; padding: 16px 32px; margin: 20px 0; }}
        .code {{ font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #1a1a2e; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #999999; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>CarProject</h1>
        </div>
        <div class='body'>
            <p>გამარჯობა,</p>
            <p>თქვენი ვერიფიკაციის კოდია:</p>
            <div class='code-box'>
                <span class='code'>{verificationCode}</span>
            </div>
            <p>ეს კოდი ძალაშია 10 წუთის განმავლობაში. თუ თქვენ არ მოგითხოვიათ ეს კოდი, უბრალოდ დააიგნორეთ ეს წერილი.</p>
        </div>
        <div class='footer'>
            © {DateTime.Now.Year} CarProject. ყველა უფლება დაცულია.
        </div>
    </div>
</body>
</html>";

        emailSender.SendEmail(request.Email, "თქვენი ვერიფიკაციის კოდი", emailBody);

        return Ok(new { message = "Registration successful" });
    }

    [HttpPost("verify-user")]
    public ActionResult VerifyUser([FromBody] VerifyUserRequest request)
    {
        var user = baza.Users.FirstOrDefault(u => u.Email == request.Email);

        if (user == null)
        {
            return NotFound("User not found.");
        }

        if (user.IsVerified)
        {
            return BadRequest("User is already verified.");
        }

        if (user.VerificationCodeExpiry < DateTime.UtcNow)
        {
            return BadRequest("Verification code has expired.");
        }

        if (user.VerificationCode != request.VerificationCode)
        {
            return BadRequest("Invalid verification code.");
        }

        user.IsVerified = true;
        user.VerificationCode = null;

        baza.SaveChanges();

        return Ok(new { message = "Email verified successfully." });
    }

    [HttpPost("login-user")]
    public ActionResult LoginUser([FromBody] LoginRequest request)
    {
        var user = baza.Users.FirstOrDefault(u => u.Email == request.Email);
        if (user == null)
        {
            return NotFound("User not found.");
        }
        if (!user.IsVerified)
        {
            return BadRequest("User is not verified.");
        }
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
        {
            return BadRequest("Invalid password.");
        }

        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        baza.SaveChanges();

        return Ok(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });
    }
    [HttpPost("refresh-token")]
    public ActionResult RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var user = baza.Users.FirstOrDefault(u => u.RefreshToken == request.RefreshToken);

        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
        {
            return Unauthorized("Invalid or expired refresh token.");
        }

        var newAccessToken = tokenService.GenerateAccessToken(user);
        var newRefreshToken = tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        baza.SaveChanges();

        return Ok(new LoginResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        });
    }
    [HttpDelete("delete-user")]
    public ActionResult DeleteUser([FromBody] string email)
    {
        var user = baza.Users.FirstOrDefault(u => u.Email == email);

        if (user == null)
        {
            return NotFound("User not found.");
        }

        baza.Users.Remove(user);
        baza.SaveChanges();

        return Ok(new { message = "User deleted successfully." });
    }

    [Authorize]
    [HttpPut("update-user")]
    public async Task<ActionResult> UpdateUser([FromForm] UpdateUserRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized("Invalid or missing user token.");

        var user = baza.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return NotFound("User not found.");

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;

        if (request.UserPhoto != null && request.UserPhoto.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(request.UserPhoto.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Only image files are allowed (jpg, jpeg, png, webp).");

            const long maxFileSize = 5 * 1024 * 1024;
            if (request.UserPhoto.Length > maxFileSize)
                return BadRequest("File size must not exceed 5MB.");

            var uploadsFolder = Path.Combine(env.WebRootPath ?? "wwwroot", "uploads", "users");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.UserPhoto.CopyToAsync(stream);
            }

            user.UserUrl = $"/uploads/users/{uniqueFileName}";
        }

        baza.SaveChanges();

        return Ok(new
        {
            id = user.Id,
            firstName = user.FirstName,
            lastName = user.LastName,
            email = user.Email,
            userUrl = user.UserUrl,
            isVerified = user.IsVerified
        });
    }
}