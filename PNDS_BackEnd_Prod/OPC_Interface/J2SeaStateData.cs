using PNDS_BackEnd_Prod.OPC_Client;
using System.ComponentModel;

namespace PNDS_BackEnd_Prod.OPC_Interface
{
    public class J2SeaStateData
    {
        
        public bool Status { get; set; } = default!;
        public float J2CurrentSpeed { get; set; } = 0; 
        public float J2CurrentDirection { get; set; } = 0; 
        public float J2MeanWave { get; set; } = 0;
        public float J2MaxWave { get; set; } = 0;
        public float J2MeanPeriod { get; set; } = 0; 
        public float J2Tide { get; set; } = 0; 

        private readonly OPCClient _opcClient;

        private BackgroundWorker J2SeaStateData_Reader;
        private Random rnd = new();
        public J2SeaStateData()
        {
#if (DEBUG)
            Console.WriteLine("Creating J2 SeaStateDataReader");
#endif
            _opcClient =  new OPCClient();
            Task t = Task.Run(() => _opcClient.Connect());
            t.Wait();

            J2SeaStateData_Reader = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            J2SeaStateData_Reader.DoWork += new DoWorkEventHandler(J2SeaStateData_Reader_DoWork);
            J2SeaStateData_Reader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(J2SeaStateData_Reader_RunWorkerCompleted);
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            J2SeaStateData_Reader.RunWorkerAsync();
#if (DEBUG)
            Console.WriteLine("Create J2SeaStateDataReader");
#endif
        }

        void J2SeaStateData_Reader_DoWork(object sender, DoWorkEventArgs e)
        {
            bool _status = true;
            if (_opcClient != null && _opcClient.OPC_Client_Connected())
            {
                float tmp;
                if (_status)
                {
                    _status = _opcClient.OPC_Read_Float("ns=1;s=t|SERVER1::Current2/Current.Speed", out tmp);
                    this.J2CurrentSpeed = (float)(Math.Ceiling(tmp * 10) / 10);
                }
                if (_status)
                {
                    _status = _opcClient.OPC_Read_Float("ns=1;s=t|SERVER1::Current2/Current.Direction", out tmp);
                    this.J2CurrentDirection = (float)(Math.Ceiling( tmp  * 10) / 10);
                }
                if (_status)
                {
                    _status = _opcClient.OPC_Read_Float("ns=1;s=t|SERVER1::WaveTide2/MeanWave.Q_PV", out tmp);
                    this.J2MeanWave = (float)(Math.Ceiling( tmp * 10) / 10);
                }
                if (_status)
                {
                    _status = _opcClient.OPC_Read_Float("ns=1;s=t|SERVER1::WaveTide2/MaxWave.Q_PV", out tmp);
                    this.J2MaxWave = (float)(Math.Ceiling( tmp * 10) / 10);
                }
                if (_status)
                {
                    _status = _opcClient.OPC_Read_Float("ns=1;s=t|SERVER1::WaveTide2/MeanPeriod.Q_PV", out tmp);
                    this.J2MeanPeriod = (float)(Math.Ceiling( tmp * 10) / 10);
                }
                if (_status)
                {
                    _status = _opcClient.OPC_Read_Float("ns=1;s=t|SERVER1::WaveTide2/Tide.Q_PV", out tmp);
                    this.J2Tide = (float)(Math.Ceiling( tmp * 10) / 10);
                }
                if ( !_status)
                {
                    this.J2CurrentSpeed = 0;
                    this.J2CurrentDirection = 0;
                    this.J2MeanWave = 0;
                    this.J2MaxWave = 0;
                    this.J2MeanPeriod = 0;
                    this.J2Tide = 0;
                }
            }
            else
            {
                this.J2CurrentSpeed = 0;
                this.J2CurrentDirection = 0;
                this.J2MeanWave = 0;
                this.J2MaxWave = 0;
                this.J2MeanPeriod = 0;
                this.J2Tide = 0;
                _status = false;
                if (_opcClient != null)
                {
                    Task t = Task.Run(() => this._opcClient.Connect());
                    t.Wait();
                }
            }

            this.Status = _status;
            Thread.Sleep(rnd.Next(1000, 1700));
        }

        private void J2SeaStateData_Reader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
#if (DEBUG)
            Console.WriteLine("J2 SeaStateDataReader  WorkComplate");
#endif
            J2SeaStateData_Reader.RunWorkerAsync();
        }
    }
}
