using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PNDS_BackEnd_Dev.Services;

namespace PNDS_BackEnd_Dev.Controllers
{
    [ApiController]
 //   [Authorize]
    [Produces("application/json")]
    [Route("/data/J2ShipData")]

    public class J2ShipDataController : ControllerBase
    {
        private readonly IJ2ShipService _shipService;
        public J2ShipDataController(IJ2ShipService shipService)
        {
            _shipService = shipService;
        }

        [HttpGet]
        public ActionResult<J2ShipData> GetShipData()
        {
            var data = _shipService.GetCurrentData();
            if (data == null)
            {
                return NotFound();
            }
            return Ok(data);
        }
    }

}
