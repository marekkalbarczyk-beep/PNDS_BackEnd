using PNDS_BackEnd_Prod.OPC_Client;
using System.ComponentModel;

namespace PNDS_BackEnd_Prod.OPC_Interface
{
    public class J2ShipData
    {
        public string J2ShipName { get; set; } = "";
        public int J2ShipDirection { get; set; } = -1;
        public bool J2Status { get; set; }  = false;

        private readonly OPCClient _opcClient;

        private BackgroundWorker J2ShipData_Reader;
        private Random rnd = new();
        public J2ShipData()
        {
#if (DEBUG)
            Console.WriteLine("J2 Creating ShipDataReader");
#endif
            _opcClient = new OPCClient();
            Task t = Task.Run(() => _opcClient.Connect());
            t.Wait();

            J2ShipData_Reader = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            J2ShipData_Reader.DoWork += new DoWorkEventHandler(J2ShipData_Reader_DoWork);
            // Dalb_Reader.ProgressChanged += new ProgressChangedEventHandler(ADAM_Reader_ProgressChanged);
            J2ShipData_Reader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(J2ShipData_Reader_RunWorkerCompleted);
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            J2ShipData_Reader.RunWorkerAsync();
#if (DEBUG)
            Console.WriteLine("Create ShipDataReader");
#endif
        }

        void J2ShipData_Reader_DoWork(object sender, DoWorkEventArgs e)
        {  
            if (_opcClient != null && _opcClient.OPC_Client_Connected())
            {
                string tmp;
                int val;
                this.J2ShipName = _opcClient.OPC_Read_String("ns=1;s=t|SERVER1::PLC/ShipData2.ShipName", out tmp) ? tmp : "Error";
                this.J2ShipDirection = _opcClient.OPC_Read_Int("ns=1;s=t|SERVER1::PLC/ShipData2.HullDirection", out val) ? val : -1;             
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

        private void J2ShipData_Reader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
#if (DEBUG)
            Console.WriteLine("J2 ShipDataReader  WorkComplate");
#endif
            J2ShipData_Reader.RunWorkerAsync();
        }
    }
}
