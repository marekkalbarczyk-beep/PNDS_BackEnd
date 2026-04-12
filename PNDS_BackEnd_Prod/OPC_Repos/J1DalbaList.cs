using PNDS_BackEnd_Prod.OPC_Interface;

namespace PNDS_BackEnd_Prod.OPC_Repos
{
    public class J1DalbaList: J1DalbaListInterface
    {

        private static readonly List<J1Dalba> J1List_Of_Dalbs = new()
        {
            new J1Dalba (1, "MD1", 3, false),
            new J1Dalba (2, "MD2", 2, false),
            new J1Dalba (3, "MD3", 2, false),
            new J1Dalba (4, "BD1", 2, false),
            new J1Dalba (5, "BD2", 2, false),
            new J1Dalba (6, "BD3", 2, false),
            new J1Dalba (7, "BD4", 2, false),
            new J1Dalba (8, "MD4", 2, false),
            new J1Dalba (9, "MD5", 2, false),
            new J1Dalba (10, "MD6", 3, false)
        };

        public IEnumerable<J1Dalba> J1GetDalbs()
        {
            return J1List_Of_Dalbs;
        }

        public J1Dalba J1GetDalb(int id)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return J1List_Of_Dalbs.Where(J1Dalba => J1Dalba.Id == id).SingleOrDefault();
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}
