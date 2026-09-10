using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PNDS_BackEnd_Prod.Models;
using PNDS_BackEnd_Prod.Services;

namespace PNDS_BackEnd_Prod.Controllers
{


    [ApiController]
    [Route("/auth")]
    public class AuthController : ControllerBase
    {

        private readonly RecaptchaService _captcha;
        private readonly ShipService _user;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public AuthController(RecaptchaService captcha, ShipService user , IConfiguration config, ILogger<AuthController> logger)
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
               
                var keyRead = _config["BearerJWT:Key"];
                if (keyRead == null)
                {
                    _logger.LogInformation("Invalid Server Key for vessel: {Username}", request.Login);
                    return Unauthorized(new { message = "Invalid Server Key" });
                }

                var key = Encoding.ASCII.GetBytes(keyRead);
                var securityKey = new SymmetricSecurityKey(key);
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, request.Login) }),
                    Expires = DateTime.UtcNow.AddHours(72),
                    SigningCredentials = credentials
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                _logger.LogInformation("Login successfull for vessel: {Username}", request.Login);
                return Ok(new { clientToken = tokenString });
               
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
