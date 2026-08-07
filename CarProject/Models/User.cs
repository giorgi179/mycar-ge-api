namespace CarProject.Models;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string? UserUrl { get; set; }
    
    public bool IsVerified { get; set; } = false;
    public string? VerificationCode { get; set; }
    public DateTime VerificationCodeExpiry { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
}