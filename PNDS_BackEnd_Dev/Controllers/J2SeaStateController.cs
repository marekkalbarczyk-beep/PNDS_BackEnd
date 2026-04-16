using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PNDS_BackEnd_Dev.Services;

namespace PNDS_BackEnd_Dev.Controllers
{
    [ApiController]
//    [Authorize]
    [Produces("application/json")]
    [Route("/data/J2SeaStateData")]

    public class J2SeaStateController : ControllerBase
    {
        private readonly IJ2SeaStateService _service;
        public J2SeaStateController(IJ2SeaStateService service)
        {
            _service = service;
        }

        [HttpGet]
        public ActionResult<J2SeaStateData> GetCurrentData()
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
