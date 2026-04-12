using PNDS_BackEnd_Prod.OPC_Client;
using System.ComponentModel;

namespace PNDS_BackEnd_Prod.OPC_Interface
{
    public class J1ShipData
    {
        public string J1ShipName { get; set; } = "";
        public bool Status { get; set; }  = false;

        private readonly OPCClient _opcClient;

        private BackgroundWorker J1ShipData_Reader;
        private Random rnd = new();
        public J1ShipData()
        {
#if (DEBUG)
            Console.WriteLine("Creating J1 ShipDataReader");
#endif
            _opcClient = new OPCClient();
            Task t = Task.Run(() => _opcClient.Connect());
            t.Wait();

            J1ShipData_Reader = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            J1ShipData_Reader.DoWork += new DoWorkEventHandler(J1ShipData_Reader_DoWork);
            // Dalb_Reader.ProgressChanged += new ProgressChangedEventHandler(ADAM_Reader_ProgressChanged);
            J1ShipData_Reader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(J1ShipData_Reader_RunWorkerCompleted);
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            J1ShipData_Reader.RunWorkerAsync();
#if (DEBUG)
            Console.WriteLine("Create J1 ShipDataReader");
#endif
        }

        void J1ShipData_Reader_DoWork(object sender, DoWorkEventArgs e)
        {  
            if (_opcClient != null && _opcClient.OPC_Client_Connected())
            {
                string tmp;
                this.J1ShipName = _opcClient.OPC_Read_String("ns=1;s=t|SERVER2::AS1/ShipData1.ShipName", out tmp) ? tmp : "Error";
                this.Status = true;
            }
            else
            {
                if (_opcClient != null)
                {
                    Task t = Task.Run(() => _opcClient.Connect());
                    t.Wait();
                }
                this.Status = false;
            }


            Thread.Sleep(rnd.Next(1000, 1350));
        }

        private void J1ShipData_Reader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
#if (DEBUG)
            Console.WriteLine("J1 ShipDataReader WorkComplate");
#endif
            J1ShipData_Reader.RunWorkerAsync();
        }
    }
}
