namespace PNDS_Auth.Models;

public class LoginRequest
{
    public string? Login { get; set; }

    public string? Password { get; set; }

    public string? CaptchaToken { get; set; }
}