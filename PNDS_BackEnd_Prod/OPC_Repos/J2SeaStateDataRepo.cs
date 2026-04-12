using PNDS_BackEnd_Prod.OPC_Interface;

namespace PNDS_BackEnd_Prod.OPC_Repos
{
    public class J2SeaStateDataRepo : J2SeaStateDataInterface
    {
        private static readonly J2SeaStateData data = new J2SeaStateData();
        public J2SeaStateData J2GetSeaStateData()
        {
            return data;
        }
    }
}
