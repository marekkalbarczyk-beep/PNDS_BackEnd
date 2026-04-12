namespace PNDS_BackEnd_Prod.Models;

public class RecaptchaResponse
{
    public bool Success { get; set; }

    public double Score { get; set; }

    //public string action { get; set; }
    //public DateTime Challenge_Ts { get; set; }
    //public string hostname { get; set; }
    //public List<string> ErrorCodes { get; set; }
}