using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PNDS_BackEnd_Prod.OPC_Interface;
using PNDS_BackEnd_Prod.OPC_Repos;


namespace PNDS_BackEnd_Prod.Controllers
{
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    [Route("/data/J1Dalba")]
    public class J1DalbController : ControllerBase
    {
        private readonly J1DalbaListInterface J1DalbaListLocal;

        public J1DalbController(J1DalbaListInterface dLI)
        {
            this.J1DalbaListLocal = dLI;
        }

        [HttpGet]
        public IEnumerable<J1Dalba> J1GetDalbas()
        {
            var dalbs = J1DalbaListLocal.J1GetDalbs();
            return dalbs;
        }

        [HttpGet("{id}")]
        public ActionResult<J1Dalba> J1GetDalba(int id)
        {
            var dalba = J1DalbaListLocal.J1GetDalb(id);
            if(dalba == null)
            {
                return NotFound();
            }
            return dalba;
        }
    }
}
