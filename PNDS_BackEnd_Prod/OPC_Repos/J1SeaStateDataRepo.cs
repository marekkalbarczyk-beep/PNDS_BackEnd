using PNDS_BackEnd_Prod.OPC_Interface;

namespace PNDS_BackEnd_Prod.OPC_Repos
{
    public class J1SeaStateDataRepo : J1SeaStateDataInterface
    {
        private static readonly J1SeaStateData data = new J1SeaStateData();
        public J1SeaStateData J1GetSeaStateData()
        {
            return data;
        }
    }
}
