using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using PNDS_Ship_Mngr.Models;
using PNDS_Ship_Mngr.Services;

namespace PNDS_Ship_Mngr.Controllers
{


    [ApiController]
    [Route("/auth")]
    public class authController : ControllerBase
    {

        private readonly IConfiguration _config;
        private readonly UserService _user;

        public authController(UserService user , IConfiguration config)
        {
            _user = user;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {

            // 2. Walidacja z pliku JSON 
            if (request.Login == null || request.Password == null)
                return Unauthorized(new { message = "Invalid credentials" });

            var isValidUser = await _user.ValidateUser(request.Login, request.Password);

            if (isValidUser)
            {
                return Ok();
               // return Ok(new { message = "Login success" });
            }
            return Unauthorized(new { message = "Invalid credentials" });
        }
    }
}
