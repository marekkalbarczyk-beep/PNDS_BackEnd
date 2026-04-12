using System.ComponentModel.DataAnnotations;
using PNDS_BackEnd_Prod.OPC_Client;
using System.ComponentModel;
using Microsoft.AspNetCore.Routing.Constraints;

namespace PNDS_BackEnd_Prod.OPC_Interface
{
    public class J1Dalba
    {
        [Required]
        public int Id = -1;
        [Required]
        public string J1Name = "";
        public int J1NoOfHooks = 0;
        public bool J1Status = default!;
        public string J1StatusMsg = default!;
        // public float[ , ] Values;
        public float[] J1Values;
        public float J1UnitLoad;

        private BackgroundWorker J1Dalb_Reader;
        private Random rnd = new();

        private readonly Dictionary<int, string> DalbMap = new ()
            {
                { 1 , "ns=1;s=t|SERVER2::M01S11/T." },
                { 2 , "ns=1;s=t|SERVER2::M01S12/D." },
                { 3 , "ns=1;s=t|SERVER2::M01S13/D." },
                { 4 , "ns=1;s=t|SERVER2::M01S14/D." },
                { 5 , "ns=1;s=t|SERVER2::M01S15/D." },
                { 6 , "ns=1;s=t|SERVER2::M01S16/D." },
                { 7 , "ns=1;s=t|SERVER2::M01S17/D." },
                { 8 , "ns=1;s=t|SERVER2::M01S18/D." },
                { 9 , "ns=1;s=t|SERVER2::M01S19/D." },
                { 10 , "ns=1;s=t|SERVER2::M01S20/T." }
        };

        private readonly OPCClient _opcClient;

        /*        public Dalba() {
                    opcClient = new OPCClient();
                    opcClient.Connect();
                    Values = new float[NoOfHooks , 6];
                    for (int i =0; i<NoOfHooks; i++)
                    {

                        float v = i + 1.6F + opcClient.OPC_Read_Float("1");
                        this.Values[i , 1] = v;
                        this.Values[i , 2] = i;
                    }

                }*/

        public J1Dalba(int id, string name, int noh, bool stst)
        {

            this.Id = id;
            this.J1Name = name;
            this.J1NoOfHooks = noh;
            this.J1Status = stst;
            J1Values = new float[J1NoOfHooks];

            _opcClient = new OPCClient();
            Task t = Task.Run(() => _opcClient.Connect());
            t.Wait();

            J1Dalb_Reader = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            J1Dalb_Reader.DoWork += new DoWorkEventHandler(J1Dalb_Reader_DoWork);
            // Dalb_Reader.ProgressChanged += new ProgressChangedEventHandler(ADAM_Reader_ProgressChanged);
            J1Dalb_Reader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(J1Dalb_Reader_RunWorkerCompleted);
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            J1Dalb_Reader.RunWorkerAsync();
#if (DEBUG)
            Console.WriteLine("Create J1 Dalb id " + this.Id.ToString());
#endif
        }


        void J1Dalb_Reader_DoWork(object sender, DoWorkEventArgs e)
        {
            bool _status = true;
            if (_opcClient != null && _opcClient.OPC_Client_Connected()) {
                float tmp;
                for (int i = 1; i <= J1NoOfHooks; i++)
                {
                    _status = _opcClient.OPC_Read_Float(DalbMap[this.Id] + "H" + i.ToString() + "_Ton", out tmp);
                    if (_status) {
                        this.J1Values[i - 1] = (float)(Math.Ceiling( tmp * 10) / 10);
                        /*                    this.Values[i - 1, 1] = _opcClient.OPC_Read_Float(DalbMap[this.Id] + "H" + i.ToString() + "_HiHiLimit", out tmp) ? tmp : -123.4f;
                                            this.Values[i - 1, 2] = _opcClient.OPC_Read_Float(DalbMap[this.Id] + "H" + i.ToString() + "_HiLimit", out tmp) ? tmp : -123.4f;
                                            // this.Values[i - 1, 3] = _opcClient.OPC_Read_Float(DalbMap[this.Id] + "H" + i.ToString() + "_");
                                            // this.Values[i - 1, 4] = _opcClient.OPC_Read_Float(DalbMap[this.Id] + "H" + i.ToString() + "_Ton");
                                            this.Values[i - 1, 5] = _opcClient.OPC_Read_Float(DalbMap[this.Id] + "H" + i.ToString() + "_BarRange", out tmp) ? tmp : -123.4f;*/
                    }
                    else {
                        this.J1Values[i - 1] = 0.0f;
                    }
                }
                if (_status) {
                    _status = _opcClient.OPC_Read_Float(DalbMap[this.Id] + "UnitLoad", out tmp);
                    this.J1UnitLoad = (float)(Math.Ceiling( tmp * 100) / 100);
                }
                else
                {
                    this.J1UnitLoad = 0.0f;
                }
            }
            else
            {
                if (_opcClient != null)
                {
                    Task t = Task.Run(() => _opcClient.Connect());
                    t.Wait();
                }
                _status = false;
            }

            this.J1Status = _status;          
            Thread.Sleep(rnd.Next(1000, 1500));
        }

        private void J1Dalb_Reader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
#if (DEBUG)
            Console.WriteLine("J1 Dalb id " + this.Id.ToString() + " WorkComplate");
#endif
            J1Dalb_Reader.RunWorkerAsync();
        }

    }
}
