using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PNDS_BackEnd_Prod.OPC_Interface;
using PNDS_BackEnd_Prod.OPC_Repos;

namespace PNDS_BackEnd_Prod.Controllers
{
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    [Route("/data/J2WeatherData")]

    public class J2WeatherDataController : ControllerBase
    {
        J2WeatherDataInterface J2WeatherDataInterfaceLocal;
        public J2WeatherDataController(J2WeatherDataInterface J2wdi) 
        {
            this.J2WeatherDataInterfaceLocal = J2wdi;
        }

        [HttpGet]
        public ActionResult<J2WeatherData> J2GetWeatherData()
        {
            var _J2WeatherData = J2WeatherDataInterfaceLocal.J2GetWeatherData();
            if (_J2WeatherData == null)
            {
                return NotFound();
            }
            return _J2WeatherData;
        }
    }
}
