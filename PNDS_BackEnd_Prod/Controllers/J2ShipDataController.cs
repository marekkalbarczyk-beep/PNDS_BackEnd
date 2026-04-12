using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PNDS_BackEnd_Prod.OPC_Interface;
using PNDS_BackEnd_Prod.OPC_Repos;

namespace PNDS_BackEnd_Prod.Controllers
{
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    [Route("/data/J2ShipData")]

    public class J2ShipDataController : ControllerBase
    {
        J2ShipDataInterface J2ShipDataInterfaceLocal;
        public J2ShipDataController(J2ShipDataInterface sdi) 
        {
            this.J2ShipDataInterfaceLocal = sdi;
        }

        [HttpGet]
        public ActionResult<J2ShipData> J2GetShipData()
        {
            var J2shipData = J2ShipDataInterfaceLocal.J2GetShipData();
            if (J2shipData == null)
            {
                return NotFound();
            }
            return J2shipData;
        }
    }
}
