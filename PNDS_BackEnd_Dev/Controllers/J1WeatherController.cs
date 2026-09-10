using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PNDS_BackEnd_Prod.Services;

namespace PNDS_BackEnd_Prod.Controllers
{
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    [Route("/data/J1WeatherData")]

    public class J1WeatherDataController : ControllerBase
    {
        private readonly IJ1WeatherService _service;
        public J1WeatherDataController(IJ1WeatherService service)
        {
            _service = service;
        }

        [HttpGet]
        public ActionResult<J1WeatherData> GetCurrentData()
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
