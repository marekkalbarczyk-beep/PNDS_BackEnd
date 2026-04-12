using PNDS_BackEnd_Prod.OPC_Interface;

namespace PNDS_BackEnd_Prod.OPC_Repos
{
    public class J2BerthingRepo : J2BerthingInterface
    {
        private static readonly J2Berthing data = new J2Berthing();
        public J2Berthing J2GetBerthing()
        {
            return data;
        }
    }
}
