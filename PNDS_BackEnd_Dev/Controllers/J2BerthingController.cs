using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PNDS_BackEnd_Dev.Services;

namespace PNDS_BackEnd_Dev.Controllers
{
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    [Route("/data/J2Berthing")]

    public class J2BerthingController : ControllerBase
    {
        private readonly IJ2BerthingService _service;
        public J2BerthingController(IJ2BerthingService service)
        {
            _service = service;
        }

        [HttpGet]
        public ActionResult<J2BerthingData> GetCurrentData()
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
