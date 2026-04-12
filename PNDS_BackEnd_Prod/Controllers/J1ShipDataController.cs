using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PNDS_BackEnd_Prod.OPC_Interface;
using PNDS_BackEnd_Prod.OPC_Repos;

namespace PNDS_BackEnd_Prod.Controllers
{
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    [Route("/data/J1ShipData")]

    public class J1ShipDataController : ControllerBase
    {
        J1ShipDataInterface J1ShipDataInterfaceLocal;
        public J1ShipDataController(J1ShipDataInterface sdi) 
        {
            this.J1ShipDataInterfaceLocal = sdi;
        }

        [HttpGet]
        public ActionResult<J1ShipData> GetShipData()
        {
            var J1shipData = J1ShipDataInterfaceLocal.J1GetShipData();
            if (J1shipData == null)
            {
                return NotFound();
            }
            return J1shipData;
        }
    }
}
