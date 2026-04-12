using PNDS_BackEnd_Prod.OPC_Interface;

namespace PNDS_BackEnd_Prod.OPC_Repos
{
    public class J1ShipDataRepo : J1ShipDataInterface
    {
        private static readonly J1ShipData data = new J1ShipData();
        public J1ShipData J1GetShipData()
        {
            return data;
        }
    }
}
