using PNDS_BackEnd_Prod.OPC_Interface;

namespace PNDS_BackEnd_Prod.OPC_Repos
{
    public class J2DalbaList: J2DalbaListInterface
    {

        private static readonly List<J2Dalba> List_Of_J2Dalbs = new()
        {
            new J2Dalba (1, "MD1", 3, false),
            new J2Dalba (2, "MD2", 3, false),
            new J2Dalba (3, "MD3", 3, false),
            new J2Dalba (4, "MD4", 3, false),
            new J2Dalba (5, "MD5", 3, false),
            new J2Dalba (6, "BD1", 3, false),
            new J2Dalba (7, "BD2", 3, false),
            new J2Dalba (8, "BD3", 3, false),
            new J2Dalba (9, "BD4", 3, false),
            new J2Dalba (10, "BD5", 3, false),
            new J2Dalba (11, "MD6", 3, false),
            new J2Dalba (12, "MD7", 3, false),
            new J2Dalba (13, "MD8", 3, false),
            new J2Dalba (14, "MD9", 3, false)
        };

        public IEnumerable<J2Dalba> J2GetDalbs()
        {
            return List_Of_J2Dalbs;
        }

        public J2Dalba J2GetDalb(int id)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return List_Of_J2Dalbs.Where(J2Dalba => J2Dalba.Id == id).SingleOrDefault();
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}
