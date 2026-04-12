using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PNDS_BackEnd_Prod.OPC_Interface;
using PNDS_BackEnd_Prod.OPC_Repos;

namespace PNDS_BackEnd_Prod.Controllers
{
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    [Route("/data/J1WeatherData")]

    public class J1WeatherDataController : ControllerBase
    {
        J1WeatherDataInterface J1WeatherDataInterfaceLocal;
        public J1WeatherDataController(J1WeatherDataInterface wdi) 
        {
            this.J1WeatherDataInterfaceLocal = wdi;
        }

        [HttpGet]
        public ActionResult<J1WeatherData> GetWeatherData()
        {
            var J1_WeatherData = J1WeatherDataInterfaceLocal.J1GetWeatherData();
            if (J1_WeatherData == null)
            {
                return NotFound();
            }
            return J1_WeatherData;
        }
    }
}
