using System.ComponentModel.DataAnnotations;
using PNDS_BackEnd_Prod.OPC_Client;
using System.ComponentModel;
using Microsoft.AspNetCore.Routing.Constraints;

namespace PNDS_BackEnd_Prod.OPC_Interface
{
    /*    public class Dalba_Struct
        {
            public float Value;
            public float HH;
            public float H;
            public float L;
            public float LL;
            public int BarMax;
        }*/
    public class J2Dalba
    {
        [Required]
        public int Id = -1;
        [Required]
        public string J2Name = "";
        public int J2NoOfHooks = 0;
        public bool J2Status = default!;
        public string J2StatusMsg = default!;
        // public float[ , ] Values;
        public float[] J2Values;
        public float J2UnitLoad;

        private BackgroundWorker J2Dalb_Reader;
        private Random rnd = new();

        private readonly Dictionary<int, string> J2DalbMap = new ()
            {
                { 1 , "ns=1;s=t|SERVER1::M02S11/T." },
                { 2 , "ns=1;s=t|SERVER1::M02S12/T." },
                { 3 , "ns=1;s=t|SERVER1::M02S13/T." },
                { 4 , "ns=1;s=t|SERVER1::M02S14/T." },
                { 5 , "ns=1;s=t|SERVER1::M02S15/T." },
                { 6 , "ns=1;s=t|SERVER1::M02S16/T." },
                { 7 , "ns=1;s=t|SERVER1::M02S17/T." },
                { 8 , "ns=1;s=t|SERVER1::M02S18/T." },
                { 9 , "ns=1;s=t|SERVER1::M02S19/T." },
                { 10 , "ns=1;s=t|SERVER1::M02S20/T." },
                { 11 , "ns=1;s=t|SERVER1::M02S21/T." },
                { 12 , "ns=1;s=t|SERVER1::M02S22/T." },
                { 13 , "ns=1;s=t|SERVER1::M02S23/T." },
                { 14 , "ns=1;s=t|SERVER1::M02S24/T." }
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

        public J2Dalba(int id, string name, int noh, bool stst)
        {

            this.Id = id;
            this.J2Name = name;
            this.J2NoOfHooks = noh;
            this.J2Status = stst;
            J2Values = new float[J2NoOfHooks];

            _opcClient = new OPCClient();
            Task t = Task.Run(() => _opcClient.Connect());
            t.Wait();

            J2Dalb_Reader = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            J2Dalb_Reader.DoWork += new DoWorkEventHandler(J2Dalb_Reader_DoWork);
            // Dalb_Reader.ProgressChanged += new ProgressChangedEventHandler(ADAM_Reader_ProgressChanged);
            J2Dalb_Reader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(J2Dalb_Reader_RunWorkerCompleted);
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            J2Dalb_Reader.RunWorkerAsync();
#if (DEBUG)
            Console.WriteLine("J2 Create Dalb id " + this.Id.ToString());
#endif
        }


        void J2Dalb_Reader_DoWork(object sender, DoWorkEventArgs e)
        {
            bool _status = true;
            if (_opcClient != null && _opcClient.OPC_Client_Connected()) {
                float tmp;
                for (int i = 1; i <= J2NoOfHooks; i++)
                {
                    _status = _opcClient.OPC_Read_Float(J2DalbMap[this.Id] + "H" + i.ToString() + "_Ton", out tmp);
                    if (_status) {
                        this.J2Values[i - 1] = (float)(Math.Ceiling( tmp * 10) / 10);
                        /*                    this.Values[i - 1, 1] = _opcClient.OPC_Read_Float(DalbMap[this.Id] + "H" + i.ToString() + "_HiHiLimit", out tmp) ? tmp : -123.4f;
                                            this.Values[i - 1, 2] = _opcClient.OPC_Read_Float(DalbMap[this.Id] + "H" + i.ToString() + "_HiLimit", out tmp) ? tmp : -123.4f;
                                            // this.Values[i - 1, 3] = _opcClient.OPC_Read_Float(DalbMap[this.Id] + "H" + i.ToString() + "_");
                                            // this.Values[i - 1, 4] = _opcClient.OPC_Read_Float(DalbMap[this.Id] + "H" + i.ToString() + "_Ton");
                                            this.Values[i - 1, 5] = _opcClient.OPC_Read_Float(DalbMap[this.Id] + "H" + i.ToString() + "_BarRange", out tmp) ? tmp : -123.4f;*/
                    }
                    else {
                        this.J2Values[i - 1] = 0.0f;
                    }
                }
                if (_status) {
                    _status = _opcClient.OPC_Read_Float(J2DalbMap[this.Id] + "UnitLoad", out tmp);
                    this.J2UnitLoad = (float)(Math.Ceiling( tmp * 100) / 100);
                }
                else
                {
                    this.J2UnitLoad = 0.0f;
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

            this.J2Status = _status;          
            Thread.Sleep(rnd.Next(1000, 1500));
        }

        private void J2Dalb_Reader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
#if (DEBUG)
            Console.WriteLine("J2 Dalb id " + this.Id.ToString() + " WorkComplate");
#endif
            J2Dalb_Reader.RunWorkerAsync();
        }

    }
}
