using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PNDS_BackEnd_Prod.OPC_Interface;
using PNDS_BackEnd_Prod.OPC_Repos;


namespace PNDS_BackEnd_Prod.Controllers
{
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    [Route("/data/J2Dalba")]
    public class J2DalbController : ControllerBase
    {
        private readonly J2DalbaListInterface J2DalbaListLocal;

        public J2DalbController(J2DalbaListInterface J2dLI)
        {
            this.J2DalbaListLocal = J2dLI;
        }

        [HttpGet]
        public IEnumerable<J2Dalba> J2GetDalbas()
        {
            var J2dalbs = J2DalbaListLocal.J2GetDalbs();
            return J2dalbs;
        }

        [HttpGet("{id}")]
        public ActionResult<J2Dalba> J2GetDalba(int id)
        {
            var J2dalba = J2DalbaListLocal.J2GetDalb(id);
            if(J2dalba == null)
            {
                return NotFound();
            }
            return J2dalba;
        }
    }
}
