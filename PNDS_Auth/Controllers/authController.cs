using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PNDS_Auth.Models;
using PNDS_Auth.Services;

namespace PNDS_Auth.Controllers
{


    [ApiController]
    [Route("/auth")]
    public class authController : ControllerBase
    {

        private readonly RecaptchaService _captcha;
        private readonly UserService _user;

        public authController(RecaptchaService captcha, UserService user)
        {
            _captcha = captcha;
            _user = user;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request.CaptchaToken == null)
                return Unauthorized(new { message = "Invalid Captcha token" });


            var captchaValid = await _captcha.Verify(request.CaptchaToken);

            if (!captchaValid)
                return BadRequest(new { message = "Captcha failed" });

            // 2. Walidacja z pliku JSON 
            if (request.Login == null || request.Password == null)
                return Unauthorized(new { message = "Invalid credentials" });

            var isValidUser = await _user.ValidateUser(request.Login, request.Password);

            if (isValidUser)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes("Twoj_Bardzo_Dlugi_I_Tajny_Klucz_Min_32_Znaki");

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, request.Login) }),
                    Expires = DateTime.UtcNow.AddHours(48),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                return Ok(new { clientToken = tokenString });
               // return Ok(new { message = "Login success" });
            }

            return Unauthorized(new { message = "Invalid credentials" });
        }
    }
}
