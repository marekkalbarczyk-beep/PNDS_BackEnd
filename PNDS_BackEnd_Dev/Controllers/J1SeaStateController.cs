using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PNDS_BackEnd_Dev.Services;

namespace PNDS_BackEnd_Dev.Controllers
{
    [ApiController]
 //   [Authorize]
    [Produces("application/json")]
    [Route("/data/J1SeaStateData")]

    public class J1SeaStateController : ControllerBase
    {
        private readonly IJ1SeaStateService _service;
        public J1SeaStateController(IJ1SeaStateService service)
        {
            _service = service;
        }

        [HttpGet]
        public ActionResult<J1SeaStateData> GetCurrentData()
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
