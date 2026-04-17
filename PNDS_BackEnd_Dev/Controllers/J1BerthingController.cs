using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PNDS_BackEnd_Dev.Services;

namespace PNDS_BackEnd_Dev.Controllers
{
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    [Route("/data/J1Berthing")]

    public class J1BerthingController : ControllerBase
    {
        private readonly IJ1BerthingService _service;
        public J1BerthingController(IJ1BerthingService service)
        {
            _service = service;
        }

        [HttpGet]
        public ActionResult<J1BerthingData> GetCurrentData()
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
