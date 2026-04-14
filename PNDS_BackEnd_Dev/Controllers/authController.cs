using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PNDS_BackEnd_Dev.Models;
using PNDS_BackEnd_Dev.Services;

namespace PNDS_BackEnd_Dev.Controllers
{


    [ApiController]
    [Route("/auth")]
    public class authController : ControllerBase
    {

        private readonly RecaptchaService _captcha;
        private readonly ShipService _user;
        private readonly IConfiguration _config;
        private readonly ILogger<authController> _logger;

        public authController(RecaptchaService captcha, ShipService user , IConfiguration config, ILogger<authController> logger)
        {
            _captcha = captcha;
            _user = user;
            _config = config;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Login attemtp for vessel: {Username}", request.Login);

            if (request.CaptchaToken == null)
            {
                _logger.LogInformation("Invalid Captcha token for vessel: {Username}", request.Login);
                return Unauthorized(new { message = "Invalid Captcha token" });
            }

            var captchaValid = await _captcha.Verify(request.CaptchaToken);

            if (!captchaValid)
            {
                _logger.LogInformation("Captcha failed for vessel: {Username}", request.Login);
                return BadRequest(new { message = "Captcha failed" });
            }

            // 2. Walidacja z pliku JSON 
            if (request.Login == null || request.Password == null)
            {
                _logger.LogInformation("Invalid credentials for vessel: {Username}", request.Login);
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var isValidUser = await _user.ValidateUser(request.Login, request.Password);

            if (isValidUser == 1)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                //var key = Encoding.ASCII.GetBytes("Twoj_Bardzo_Dlugi_I_Tajny_Klucz_Min_32_Znaki");
                var keyRead = _config["BearerJWT:Key"];
                if (keyRead == null)
                {
                    _logger.LogInformation("Invalid Server Key for vessel: {Username}", request.Login);
                    return Unauthorized(new { message = "Invalid Server Key" });
                }

                var key = Encoding.ASCII.GetBytes(keyRead);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, request.Login) }),
                    Expires = DateTime.UtcNow.AddHours(72),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                _logger.LogInformation("Login successfull for vessel: {Username}", request.Login);
                return Ok(new { clientToken = tokenString });
               // return Ok(new { message = "Login success" });
            }
            switch (isValidUser)
            {
                case -1:
                    _logger.LogInformation("Unknown Login for vessel: {Username}", request.Login);
                    return Unauthorized(new { message = "Unknown Login" });
                case -2:
                    _logger.LogInformation("Wrong password for vessel: {Username}", request.Login);
                    return Unauthorized(new { message = "Wrong password" });
                case -3:
                    _logger.LogInformation("No expiring date for vessel: {Username}", request.Login);
                    return Unauthorized(new { message = "Account expired" });
                case -4:
                    _logger.LogInformation("Account expired for vessel: {Username}", request.Login);
                    return Unauthorized(new { message = "Account expired" });
                default:
                    _logger.LogInformation("Unknown error for vessel: {Username}", request.Login);
                    return Unauthorized(new { message = "Unknown error" });

            }
        }
    }
}
