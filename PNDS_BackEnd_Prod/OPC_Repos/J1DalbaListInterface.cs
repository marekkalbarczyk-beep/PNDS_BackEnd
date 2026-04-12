using PNDS_BackEnd_Prod.OPC_Interface;

namespace PNDS_BackEnd_Prod.OPC_Repos
{
    public interface J1DalbaListInterface
    {
        IEnumerable<J1Dalba> J1GetDalbs();

        J1Dalba J1GetDalb(int id);

    }
}
