namespace PNDS_BackEnd_Dev.Models;

public class LoginRequest
{
    public string? Login { get; set; }

    public string? Password { get; set; }

    public string? CaptchaToken { get; set; }
}