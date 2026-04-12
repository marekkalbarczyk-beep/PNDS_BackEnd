using PNDS_BackEnd_Prod.OPC_Client;
using System.ComponentModel;

namespace PNDS_BackEnd_Prod.OPC_Interface
{
    public class J1SeaStateData
    {
        
        public bool Status { get; set; } = default!;
        public float J1CurrentSpeed { get; set; } = 0; 
        public float J1CurrentDirection { get; set; } = 0; 
        public float J1MeanWave { get; set; } = 0;
        public float J1MaxWave { get; set; } = 0;
        public float J1MeanPeriod { get; set; } = 0; 
        public float J1Tide { get; set; } = 0; 

        private readonly OPCClient _opcClient;

        private BackgroundWorker J1SeaStateData_Reader;
        private Random rnd = new();
        public J1SeaStateData()
        {
#if (DEBUG)
            Console.WriteLine("Creating J1 SeaStateDataReader");
#endif
            _opcClient =new OPCClient();
            Task t = Task.Run(() => _opcClient.Connect());
            t.Wait();

            J1SeaStateData_Reader = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            J1SeaStateData_Reader.DoWork += new DoWorkEventHandler(J1SeaStateData_Reader_DoWork);
            // Dalb_Reader.ProgressChanged += new ProgressChangedEventHandler(ADAM_Reader_ProgressChanged);
            J1SeaStateData_Reader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(J1SeaStateData_Reader_RunWorkerCompleted);
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            J1SeaStateData_Reader.RunWorkerAsync();
            Console.WriteLine("Create J1 SeaStateDataReader");
        }

        void J1SeaStateData_Reader_DoWork(object sender, DoWorkEventArgs e)
        {
            bool _status = true;
            if (_opcClient != null && _opcClient.OPC_Client_Connected())
            {
                float tmp;
                if (_status)
                {
                    _status = _opcClient.OPC_Read_Float("ns=1;s=t|SERVER2::Current1/Current.Speed", out tmp);
                    this.J1CurrentSpeed = (float)(Math.Ceiling(tmp * 10) / 10);
                }
                if (_status)
                {
                    _status = _opcClient.OPC_Read_Float("ns=1;s=t|SERVER2::Current1/Current.Direction", out tmp);
                    this.J1CurrentDirection = (float)(Math.Ceiling( tmp  * 10) / 10);
                }
                if (_status)
                {
                    _status = _opcClient.OPC_Read_Float("ns=1;s=t|SERVER2::WaveTide1/MeanWave.Q_PV", out tmp);
                    this.J1MeanWave = (float)(Math.Ceiling( tmp * 10) / 10);
                }
                if (_status)
                {
                    _status = _opcClient.OPC_Read_Float("ns=1;s=t|SERVER2::WaveTide1/MaxWave.Q_PV", out tmp);
                    this.J1MaxWave = (float)(Math.Ceiling( tmp * 10) / 10);
                }
                if (_status)
                {
                    _status = _opcClient.OPC_Read_Float("ns=1;s=t|SERVER2::WaveTide1/MeanPeriod.Q_PV", out tmp);
                    this.J1MeanPeriod = (float)(Math.Ceiling( tmp * 10) / 10);
                }
                if (_status)
                {
                    _status = _opcClient.OPC_Read_Float("ns=1;s=t|SERVER2::WaveTide1/Tide.Q_PV", out tmp);
                    this.J1Tide = (float)(Math.Ceiling( tmp * 10) / 10);
                }
                if ( !_status)
                {
                    this.J1CurrentSpeed = 0;
                    this.J1CurrentDirection = 0;
                    this.J1MeanWave = 0;
                    this.J1MaxWave = 0;
                    this.J1MeanPeriod = 0;
                    this.J1Tide = 0;
                }
            }
            else
            {
                this.J1CurrentSpeed = 0;
                this.J1CurrentDirection = 0;
                this.J1MeanWave = 0;
                this.J1MaxWave = 0;
                this.J1MeanPeriod = 0;
                this.J1Tide = 0;
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

        private void J1SeaStateData_Reader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
#if (DEBUG)
            Console.WriteLine("J1 SeaStateDataReader WorkComplate");
#endif
            J1SeaStateData_Reader.RunWorkerAsync();
        }
    }
}
