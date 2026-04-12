using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PNDS_BackEnd_Prod.OPC_Interface;
using PNDS_BackEnd_Prod.OPC_Repos;

namespace PNDS_BackEnd_Prod.Controllers
{
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    [Route("/data/J1SeaStateData")]

    public class J1SeaStateDataController : ControllerBase
    {
        J1SeaStateDataInterface J1SeaStateDataInterfaceLocal;
        public J1SeaStateDataController(J1SeaStateDataInterface ssdi) 
        {
            this.J1SeaStateDataInterfaceLocal = ssdi;
        }

        [HttpGet]
        public ActionResult<J1SeaStateData> J1GetSeaStateData()
        {
            var _SeaStateData = J1SeaStateDataInterfaceLocal.J1GetSeaStateData();
            if (_SeaStateData == null)
            {
                return NotFound();
            }
            return _SeaStateData;
        }
    }
}
