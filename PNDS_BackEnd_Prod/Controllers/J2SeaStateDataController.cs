using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PNDS_BackEnd_Prod.OPC_Interface;
using PNDS_BackEnd_Prod.OPC_Repos;

namespace PNDS_OPPNDS_BackEnd_ProdC_Web_Api.Controllers
{
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    [Route("/data/J2SeaStateData")]

    public class J2SeaStateDataController : ControllerBase
    {
        J2SeaStateDataInterface J2SeaStateDataInterfaceLocal;
        public J2SeaStateDataController(J2SeaStateDataInterface ssdi) 
        {
            this.J2SeaStateDataInterfaceLocal = ssdi;
        }

        [HttpGet]
        public ActionResult<J2SeaStateData> J2GetSeaStateData()
        {
            var _J2SeaStateData = J2SeaStateDataInterfaceLocal.J2GetSeaStateData();
            if (_J2SeaStateData == null)
            {
                return NotFound();
            }
            return _J2SeaStateData;
        }
    }
}
