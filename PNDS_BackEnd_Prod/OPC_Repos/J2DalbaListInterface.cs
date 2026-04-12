using PNDS_BackEnd_Prod.OPC_Interface;

namespace PNDS_BackEnd_Prod.OPC_Repos
{
    public interface J2DalbaListInterface
    {
        IEnumerable<J2Dalba> J2GetDalbs();

        J2Dalba J2GetDalb(int id);

    }
}
