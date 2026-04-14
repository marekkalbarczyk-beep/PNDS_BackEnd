using System.Text.Json;
using PNDS_BackEnd_Dev.Controllers;
using PNDS_BackEnd_Dev.Models;

namespace PNDS_BackEnd_Dev.Services;
public class RecaptchaService
{
    private readonly HttpClient _client;
    private readonly IConfiguration _config;
    private readonly ILogger<RecaptchaService> _logger;

    public RecaptchaService(HttpClient client, IConfiguration config, ILogger<RecaptchaService> logger)
    {
        _client = client;
        _config = config;
        _logger = logger;
    }

    public async Task<bool> Verify(string token)
    {
        var secret = _config["Recaptcha:SecretKey"];

        var response = await _client.PostAsync(
            $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={token}",
            null
        );

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("reCAPTCHA response failed");
            return false;
        }
        var result = await response.Content.ReadFromJsonAsync<RecaptchaResponse>();

        if (result == null)
        {
            _logger.LogWarning("reCAPTCHA result is null");
            return false;
        }
        // W v3 wynik 0.5 to zazwyczaj "człowiek", ale możesz to dostosować
        //Console.WriteLine(DateTime.Now.ToString() + "reCAPTCHA score" + result.Score);
        _logger.LogInformation("reCAPTCHA score" + result.Score);
        return result.Success && result.Score >= 0.5;

      
    }
}