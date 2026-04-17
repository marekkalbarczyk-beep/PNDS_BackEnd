using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PNDS_BackEnd_Dev.Services;

namespace PNDS_BackEnd_Dev.Controllers
{
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    [Route("/data/J1Mooring")]

    public class J1MooringController : ControllerBase
    {
        private readonly IJ1MooringListService _mooringList;

        public J1MooringController(IJ1MooringListService mooringList)
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
