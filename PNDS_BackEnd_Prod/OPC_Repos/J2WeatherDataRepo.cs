using PNDS_BackEnd_Prod.OPC_Interface;

namespace PNDS_BackEnd_Prod.OPC_Repos
{
    public class J2WeatherDataRepo : J2WeatherDataInterface
    {
        private static readonly J2WeatherData data = new J2WeatherData();
        public J2WeatherData J2GetWeatherData()
        {
            return data;
        }
    }
}
