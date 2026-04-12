using System.Text.Json;
using PNDS_Auth.Models;

namespace PNDS_Auth.Services;
public class RecaptchaService
{
    private readonly HttpClient _client;
    private readonly IConfiguration _config;

    public RecaptchaService(HttpClient client, IConfiguration config)
    {
        _client = client;
        _config = config;
    }

    public async Task<bool> Verify(string token)
    {
        var secret = _config["Recaptcha:SecretKey"];

        var response = await _client.PostAsync(
            $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={token}",
            null
        );

        if (!response.IsSuccessStatusCode) return false;

        var result = await response.Content.ReadFromJsonAsync<RecaptchaResponse>();

        if (result == null) return false;   
        // W v3 wynik 0.5 to zazwyczaj "człowiek", ale możesz to dostosować
        return result.Success && result.Score >= 0.5;

      
    }
}