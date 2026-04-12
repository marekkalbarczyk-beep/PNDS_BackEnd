using System.Collections.Specialized;
using Microsoft.AspNetCore.Mvc;
using PNDS_Ship_Mngr.Interfaces;
using PNDS_Ship_Mngr.Models;

namespace PNDS_Ship_Mngr.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("/ships")]
    public class shipDataController : ControllerBase
    {
        private  shipListInterface shipListLocal;

        public shipDataController(shipListInterface sDC)
        {
            shipListLocal = sDC;
        }

        [HttpGet]
        public IEnumerable<shipDataDto> GetShips()
        {
            var _shipList = shipListLocal.GetShips().Select(ship => new shipDataDto
            {
                shipName = ship.shipName,
                shipOwner = ship.shipOwner,
                shipExpire = ship.shipExpire
            }).ToList();
            return _shipList;
        }

        [HttpGet("{shipName}")]
        public ActionResult<shipDataDto> GetShip(string shipName)
        {
            var _ship = shipListLocal.GetShip(shipName);
           
            if (_ship is null)
            {
                return NotFound();
            }
            var _shipResp = new shipDataDto
            {
                shipName = _ship.shipName,
                shipExpire = _ship.shipExpire,
                shipOwner = _ship.shipOwner
            };
            return _shipResp;
        }

        [HttpPut("{shipName}")]
        public ActionResult<shipData> SetShip(string shipName, [FromBody] shipUpdateInterface putdata)
        {
            var _ship = shipListLocal.GetShip(shipName);
            if (_ship is null)  //create new shipData
            {
                if (putdata.shipPassword == null)
                {
                    return BadRequest();
                }
                else if (shipListLocal.CreateShip(shipName, putdata.shipPassword, putdata.shipExpire, putdata.shipOwner))
                {
                    _ship = shipListLocal.GetShip(shipName);
                    var _shipResp = new shipDataDto
                    {
                        shipName = _ship.shipName,
                        shipExpire = _ship.shipExpire,
                        shipOwner = _ship.shipOwner
                    };
                    return Ok(_shipResp);
                }
                else
                {
                    return BadRequest();
                }

            }
            else  //update existing shipData
            {

                if (putdata.shipPassword != null)
                {
                    if (shipListLocal.UpdateShipPass(shipName, putdata.shipPassword, putdata.shipExpire, putdata.shipOwner))
                    {
                        _ship = shipListLocal.GetShip(shipName);
                        var _shipResp = new shipDataDto
                        {
                            shipName = _ship.shipName,
                            shipExpire = _ship.shipExpire,
                            shipOwner = _ship.shipOwner
                        };
                        return Ok(_shipResp);
                    }
                    else
                        return BadRequest();
                }
                else
                {
                    if (shipListLocal.UpdateShipNoPass(shipName, putdata.shipExpire, putdata.shipOwner))
                    {
                        _ship = shipListLocal.GetShip(shipName);
                        var _shipResp = new shipDataDto
                        {
                            shipName = _ship.shipName,
                            shipExpire = _ship.shipExpire,
                            shipOwner = _ship.shipOwner
                        };
                        return Ok(_shipResp);
                    }
                    else
                        return BadRequest();
                }
            }
        }

        [HttpDelete("{shipName}")]
        public ActionResult<shipDataDto> DeleteShip(string shipName)
        {
            var _ship = shipListLocal.GetShip(shipName);
            if (_ship is null) 
            {
                return NotFound();
            }
            shipListLocal.DeleteShip(shipName);
            var _shipResp = new shipDataDto
            {
                shipName = _ship.shipName,
                shipExpire = _ship.shipExpire,
                shipOwner = _ship.shipOwner
            };
            return Ok(_shipResp);
        }
    }
}
