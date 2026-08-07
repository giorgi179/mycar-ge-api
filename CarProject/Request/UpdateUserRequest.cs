namespace CarProject.Request;
public class UpdateUserRequest
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public IFormFile? UserPhoto { get; set; }
}
