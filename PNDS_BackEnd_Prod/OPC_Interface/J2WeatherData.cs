using PNDS_BackEnd_Prod.OPC_Client;
using System.ComponentModel;

namespace PNDS_BackEnd_Prod.OPC_Interface
{
    public class J2WeatherData
    {
        
        public bool Status = default!;
        public float J2WD { get; set; } = 0; //Wind Direction
        public float J2WS { get; set; } = 0; //Wind Speed
        public float J2P { get; set; } = 0; // Preasure
        public float J2T { get; set; } = 0; //Temperature
        public float J2H { get; set; } = 0; //Humanidy
        public float J2R { get; set; } = 0; //Rain

        private readonly OPCClient _opcClient;

        private BackgroundWorker J2WeatherData_Reader;
        private Random rnd = new();
        public J2WeatherData()
        {
#if (DEBUG)
            Console.WriteLine("Creating WeatherDataReader");
#endif
            _opcClient = new OPCClient();
            Task t = Task.Run(() => _opcClient.Connect());
            t.Wait();

            J2WeatherData_Reader = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            J2WeatherData_Reader.DoWork += new DoWorkEventHandler(J2WeatherData_Reader_DoWork);
            // Dalb_Reader.ProgressChanged += new ProgressChangedEventHandler(ADAM_Reader_ProgressChanged);
            J2WeatherData_Reader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(J2WeatherData_Reader_RunWorkerCompleted);
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            J2WeatherData_Reader.RunWorkerAsync();
#if (DEBUG)
            Console.WriteLine("Create WeatherDataReader");
#endif
        }

        void J2WeatherData_Reader_DoWork(object sender, DoWorkEventArgs e)
        {  
            if (_opcClient != null && _opcClient.OPC_Client_Connected())
            {
                float tmp;
                this.J2T = (float)(Math.Ceiling((_opcClient.OPC_Read_Float("ns=1;s=t|SERVER1::AirTemperature/MEASURE.Q_PV", out tmp) ? tmp : -123.4f) * 10) / 10);
                this.J2P = (float)(Math.Ceiling((_opcClient.OPC_Read_Float("ns=1;s=t|SERVER1::AtmPressure/MEASURE.Q_PV", out tmp) ? tmp : -123.4f) * 10) / 10);
                this.J2H = (float)(Math.Ceiling((_opcClient.OPC_Read_Float("ns=1;s=t|SERVER1::Humidity/MEASURE.Q_PV", out tmp) ? tmp : -123.4f) * 10) / 10);
                this.J2WD = (float)(Math.Ceiling((_opcClient.OPC_Read_Float("ns=1;s=t|SERVER1::Wind/WIND_COMPASS.Direction", out tmp) ? tmp : -123.4f) * 10) / 10);
                this.J2WS = (float)(Math.Ceiling((_opcClient.OPC_Read_Float("ns=1;s=t|SERVER1::Wind/WIND_COMPASS.Speed", out tmp) ? tmp : -123.4f) * 10) / 10);
                this.J2R = (float)(Math.Ceiling((_opcClient.OPC_Read_Float("ns=1;s=t|SERVER1::Rain/MEASURE.Q_PV", out tmp) ? tmp : -123.4f) * 10) / 10);
            }
            else
            {
                if (_opcClient != null)
                {
                    Task t = Task.Run(() => this._opcClient.Connect());
                    t.Wait();
                }
            }


            Thread.Sleep(rnd.Next(1000, 1300));
        }

        private void J2WeatherData_Reader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
#if (DEBUG)
            Console.WriteLine("J2 WeatherDataReader  WorkComplate");
#endif
            J2WeatherData_Reader.RunWorkerAsync();
        }
    }
}
