using PNDS_BackEnd_Prod.OPC_Interface;

namespace PNDS_BackEnd_Prod.OPC_Repos
{
    public class J2ShipDataRepo : J2ShipDataInterface
    {
        private static readonly J2ShipData data = new J2ShipData();
        public J2ShipData J2GetShipData()
        {
            return data;
        }
    }
}
