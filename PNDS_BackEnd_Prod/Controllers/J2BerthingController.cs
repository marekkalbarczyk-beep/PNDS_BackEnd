using Microsoft.AspNetCore.Mvc;
using PNDS_BackEnd_Prod.OPC_Interface;
using PNDS_BackEnd_Prod.OPC_Repos;

namespace PNDS_BackEnd_Prod.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("/data/J2Berthing")]

    public class J2BerthingController : ControllerBase
    {
        J2BerthingInterface J2BerthingInterfaceLocal;
        public J2BerthingController(J2BerthingInterface sdi) 
        {
            this.J2BerthingInterfaceLocal = sdi;
        }

        [HttpGet]
        public ActionResult<J2Berthing> J2GetBerthing()
        {
            var J2Berthing = J2BerthingInterfaceLocal.J2GetBerthing();
            if (J2Berthing == null)
            {
                return NotFound();
            }
            return J2Berthing;
        }
    }
}
