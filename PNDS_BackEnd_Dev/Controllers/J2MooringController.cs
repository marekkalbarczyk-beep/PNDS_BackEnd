using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PNDS_BackEnd_Dev.Services;

namespace PNDS_BackEnd_Dev.Controllers
{
    [ApiController]
    //[Authorize]
    [Produces("application/json")]
    [Route("/data/J2Mooring")]

    public class J2MooringController : ControllerBase
    {
        private readonly IJ2MooringListService _mooringList;

        public J2MooringController(IJ2MooringListService mooringList)
        {
            _mooringList = mooringList;
        }

        // Pobierz wszystkie (budzi wszystkie serwisy)
        [HttpGet]
        public IActionResult GetAll() => Ok(_mooringList.GetCurrentData());

        // Pobierz konkretny (budzi tylko ten jeden id)
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var data = _mooringList.GetCurrentData(id);
            return data != null ? Ok(data) : NotFound();
        }

    }

}
