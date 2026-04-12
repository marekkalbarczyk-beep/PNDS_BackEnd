using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using PNDS_Proxt.OPC_Client;

namespace PNDS_Proxy
{
    public partial class PNDS_Proxy_Service : ServiceBase
    {

        //Timers
        private static System.Timers.Timer timerOPCReader;
        private static readonly OPCClient opcPNDSClient;
        private static bool opcClientBusy = false;
        public PNDS_Proxy_Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            timerOPCReader = new System.Timers.Timer(11000);
            timerOPCReader.Enabled = true;
            timerOPCReader.Elapsed += new ElapsedEventHandler(timerOPCReader_Tick);
        }

        private async void timerOPCReader_Tick(object sender, EventArgs e)
        {
            if (!opcClientBusy)
            {
                opcClientBusy = true;
                if (opcPNDSClient.OPC_Client_Connected())
                {

                }
                else
                {
                    try
                    {
                        await opcPNDSClient.Connect();
                    }
                    catch (Exception ex)
                    {
                        Log("timerOPCReader_Tick  opcPNDSClient.Connect: " + ex.Message);
                    }
                    finally 
                    {

                    }
                }

                opcClientBusy = false;
            }

        }

        protected override void OnStop()
        {

        }

        static void Log(string logEntry)
        {

            String logFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\Logs\\wms2phd.log";
            try
            {
                using (StreamWriter logWriter = new StreamWriter(logFile, true))
                {
                    logWriter.WriteLineAsync(DateTime.Now.ToString() + "\t" + logEntry);
                    logWriter.Close();
                }
            }
            catch (Exception) { }
        }

    }
}
