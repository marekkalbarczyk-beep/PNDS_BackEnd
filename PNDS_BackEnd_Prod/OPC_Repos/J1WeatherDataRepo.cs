using PNDS_BackEnd_Prod.OPC_Interface;

namespace PNDS_BackEnd_Prod.OPC_Repos
{
    public class J1WeatherDataRepo : J1WeatherDataInterface
    {
        private static readonly J1WeatherData data = new J1WeatherData();
        public J1WeatherData J1GetWeatherData()
        {
            return data;
        }
    }
}
