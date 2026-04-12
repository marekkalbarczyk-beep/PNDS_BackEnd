using PNDS_BackEnd_Prod.OPC_Client;
using System.ComponentModel;

namespace PNDS_BackEnd_Prod.OPC_Interface
{
    public class J2Berthing
    {
        public int J2Angle { get; set; } = 0;
        public float J2Laser_R_Speed { get; set; } = 0.0f;
        public float J2Laser_R_Distance { get; set; } = 0.0f;
        public float J2Laser_L_Speed { get; set; } = 0.0f;
        public float J2Laser_L_Distance { get; set; } = 0.0f;

        public bool J2Status { get; set; }  = false;

        private readonly OPCClient _opcClient;

        private BackgroundWorker J2Berthing_Reader;
        private Random rnd = new();
        public J2Berthing()
        {
#if (DEBUG)
            Console.WriteLine("J2 Creating J2Berthing");
#endif
            _opcClient = new OPCClient();
            Task t = Task.Run(() => _opcClient.Connect());
            t.Wait();

            J2Berthing_Reader = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            J2Berthing_Reader.DoWork += new DoWorkEventHandler(J2Berthing_Reader_DoWork);
            // Dalb_Reader.ProgressChanged += new ProgressChangedEventHandler(ADAM_Reader_ProgressChanged);
            J2Berthing_Reader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(J2Berthing_Reader_RunWorkerCompleted);
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            J2Berthing_Reader.RunWorkerAsync();
#if (DEBUG)
            Console.WriteLine("Create J2BerthingReader");
#endif
        }

        void J2Berthing_Reader_DoWork(object sender, DoWorkEventArgs e)
        {  
            if (_opcClient != null && _opcClient.OPC_Client_Connected())
            {
                int val;
                float fval_rd = 0;
                float fval_ld = 0;
                float fval_rs = 0;
                float fval_ls = 0;
                this.J2Angle = _opcClient.OPC_Read_Int("ns=1;s=t|SERVER1::B02ANGLE/ANGLE.Angle", out val) ? val : -1;
                this.J2Laser_R_Distance = _opcClient.OPC_Read_Float("ns=1;s=t|SERVER1::B02LASER_RIGHT/DISTANCE.DistReal_m", out fval_rd) ? fval_rd : -1.0f;
                this.J2Laser_L_Distance = _opcClient.OPC_Read_Float("ns=1;s=t|SERVER1::B02LASER_LEFT/DISTANCE.DistReal_m", out fval_ld) ? fval_ld : -1.0f;
                this.J2Laser_R_Speed = _opcClient.OPC_Read_Float("ns=1;s=t|SERVER1::B02LASER_RIGHT/SPEED.Vsgn", out fval_rs) ? fval_rs : -1.0f;
                this.J2Laser_L_Speed = _opcClient.OPC_Read_Float("ns=1;s=t|SERVER1::B02LASER_LEFT/SPEED.Vsgn", out fval_ls) ? fval_ls : -1.0f;
                this.J2Status = true;
            }
            else
            {
                if (_opcClient != null)
                {
                    Task t = Task.Run(() => _opcClient.Connect());
                    t.Wait();
                }
                this.J2Status = false;
            }


            Thread.Sleep(rnd.Next(1000, 1350));
        }

        private void J2Berthing_Reader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
#if (DEBUG)
            Console.WriteLine("J2 Berthing  WorkComplate");
#endif
            J2Berthing_Reader.RunWorkerAsync();
        }
    }
}
