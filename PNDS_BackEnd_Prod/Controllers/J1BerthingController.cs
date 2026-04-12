using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PNDS_BackEnd_Prod.OPC_Interface;
using PNDS_BackEnd_Prod.OPC_Repos;

namespace PNDS_BackEnd_Prod.Controllers
{
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    [Route("/data/J1Berthing")]

    public class J1BerthingController : ControllerBase
    {
        J1BerthingInterface J1BerthingInterfaceLocal;
        public J1BerthingController(J1BerthingInterface sdi) 
        {
            this.J1BerthingInterfaceLocal = sdi;
        }

        [HttpGet]
        public ActionResult<J1Berthing> J1GetBerthing()
        {
            var J1Berthing = J1BerthingInterfaceLocal.J1GetBerthing();
            if (J1Berthing == null)
            {
                return NotFound();
            }
            return J1Berthing;
        }
    }
}
