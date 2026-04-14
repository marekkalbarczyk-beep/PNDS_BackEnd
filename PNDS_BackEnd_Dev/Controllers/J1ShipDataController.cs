using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PNDS_BackEnd_Dev.Services;

namespace PNDS_BackEnd_Dev.Controllers
{
    [ApiController]
//    [Authorize]
    [Produces("application/json")]
    [Route("/data/J1ShipData")]

    public class J1ShipDataController : ControllerBase
    {
        private readonly IJ1ShipService _shipService;
        public J1ShipDataController(IJ1ShipService shipService)
        {
            _shipService = shipService;
        }

        [HttpGet]
        public ActionResult<J1ShipData> GetShipData()
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
