using PNDS_BackEnd_Prod.OPC_Interface;

namespace PNDS_BackEnd_Prod.OPC_Repos
{
    public class J1BerthingRepo : J1BerthingInterface
    {
        private static readonly J1Berthing data = new J1Berthing();
        public J1Berthing J1GetBerthing()
        {
            return data;
        }
    }
}
