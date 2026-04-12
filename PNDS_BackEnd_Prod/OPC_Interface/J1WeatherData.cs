using PNDS_BackEnd_Prod.OPC_Client;
using System.ComponentModel;

namespace PNDS_BackEnd_Prod.OPC_Interface
{
    public class J1WeatherData
    {
        
        public bool Status = default!;
        public float J1WD { get; set; } = 0; //Wind Direction
        public float J1WS { get; set; } = 0; //Wind Speed
        public float J1P { get; set; } = 0; // Preasure
        public float J1T { get; set; } = 0; //Temperature
        public float J1H { get; set; } = 0; //Humanidy
        public float J1R { get; set; } = 0; //Rain

        private OPCClient _opcClient;

        private BackgroundWorker J1WeatherData_Reader;
        private Random rnd = new();
        public J1WeatherData()
        {
#if (DEBUG)
            Console.WriteLine("Creating J1 WeatherDataReader");
#endif
            _opcClient = new OPCClient();
            Task t = Task.Run(() => _opcClient.Connect());
            t.Wait();

            J1WeatherData_Reader = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            J1WeatherData_Reader.DoWork += new DoWorkEventHandler(J1WeatherData_Reader_DoWork);
            // Dalb_Reader.ProgressChanged += new ProgressChangedEventHandler(ADAM_Reader_ProgressChanged);
            J1WeatherData_Reader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(J1WeatherData_Reader_RunWorkerCompleted);
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            J1WeatherData_Reader.RunWorkerAsync();
#if (DEBUG)
            Console.WriteLine("Create J1 WeatherDataReader");
#endif
        }

        void J1WeatherData_Reader_DoWork(object sender, DoWorkEventArgs e)
        {  
            if (_opcClient != null && _opcClient.OPC_Client_Connected())
            {
                float tmp;
                this.J1T = (float)(Math.Ceiling((_opcClient.OPC_Read_Float("ns=1;s=t|SERVER2::AirTemperature1/MEASURE.Q_PV", out tmp) ? tmp : -123.4f) * 10) / 10);
                this.J1P = (float)(Math.Ceiling((_opcClient.OPC_Read_Float("ns=1;s=t|SERVER2::AtmPressure1/MEASURE.Q_PV", out tmp) ? tmp : -123.4f) * 10) / 10);
                this.J1H = (float)(Math.Ceiling((_opcClient.OPC_Read_Float("ns=1;s=t|SERVER2::Humidity1/MEASURE.Q_PV", out tmp) ? tmp : -123.4f) * 10) / 10);
                this.J1WD = (float)(Math.Ceiling((_opcClient.OPC_Read_Float("ns=1;s=t|SERVER2::Wind1/WIND_COMPASS.Direction", out tmp) ? tmp : -123.4f) * 10) / 10);
                this.J1WS = (float)(Math.Ceiling((_opcClient.OPC_Read_Float("ns=1;s=t|SERVER2::Wind1/WIND_COMPASS.Speed", out tmp) ? tmp : -123.4f) * 10) / 10);
                this.J1R = (float)(Math.Ceiling((_opcClient.OPC_Read_Float("ns=1;s=t|SERVER2::Rain1/MEASURE.Q_PV", out tmp) ? tmp : -123.4f) * 10) / 10);
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

        private void J1WeatherData_Reader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
#if (DEBUG)
            Console.WriteLine("J1 WeatherDataReader  WorkComplate");
#endif
            J1WeatherData_Reader.RunWorkerAsync();
        }
    }
}
