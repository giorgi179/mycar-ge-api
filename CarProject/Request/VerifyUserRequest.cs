namespace CarProject.Request;

public class VerifyUserRequest
{
    public string Email { get; set; }
    public string VerificationCode { get; set; }
}