using PNDS_Ship_Mngr.Models;

namespace PNDS_Ship_Mngr.Interfaces
{
    public interface shipListInterface
    {
        IEnumerable<shipData> GetShips();
        shipData GetShip(string shipName);
        bool UpdateShipPass(string shipName, string shipPassword, DateTime shipExpire, string shipOwner);
        bool UpdateShipNoPass(string shipName, DateTime shipExpire, string shipOwner);
        bool CreateShip(string shipName, string shipPassword, DateTime shipExpire, string shipOwner);
        bool DeleteShip(string shipName);
    }
}
