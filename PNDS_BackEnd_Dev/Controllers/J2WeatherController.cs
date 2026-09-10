using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PNDS_BackEnd_Dev.Services;

namespace PNDS_BackEnd_Dev.Controllers
{
    [ApiController]
 //   [Authorize]
    [Produces("application/json")]
    [Route("/data/J2WeatherData")]

    public class J2WeatherDataController : ControllerBase
    {
        private readonly IJ2WeatherService _service;
        public J2WeatherDataController(IJ2WeatherService service)
        {
            _service = service;
        }

        [HttpGet]
        public ActionResult<J2WeatherData> GetCurrentData()
        {
            var data = _service.GetCurrentData();
            if (data == null)
            {
                return NotFound();
            }
            return Ok(data);
        }
    }

}
