using Microsoft.AspNetCore.Http;

namespace CarProject.Request;

public class UserRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string ReEnterPassword { get; set; }
    public IFormFile? UserPhoto { get; set; }

}