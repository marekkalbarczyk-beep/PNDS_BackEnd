using Opc.Ua.Configuration;
using Opc.Ua;
using System.Threading;
using System.Threading.Tasks;
using Opc.Ua.Client;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using System;

namespace PNDS_Proxt.OPC_Client
{
    public class OPCClient
    {

        private static string password = null;
        private EndpointDescription endpointDescription = null;
        private readonly bool useSecurity = false;
        
        private static string applicationName = "PNDS_OPC_Client";
        private static string configSectionName = "PNDS.OPC_Client";

        private ITransportWaitingConnection connection = null;
        private uint SessionLifeTime = 60 * 1000;
        //IUserIdentity UserIdentity = new UserIdentity(username, userpassword ?? string.Empty);
        private CancellationToken ct = default;

        // Define the UA Client application
        private static CertificatePasswordProvider PasswordProvider = new CertificatePasswordProvider(password);
        private ApplicationInstance AppInstance = new ApplicationInstance
        {
            ApplicationName = applicationName,
            ApplicationType = ApplicationType.Client,
            ConfigSectionName = configSectionName,
            CertificatePasswordProvider = PasswordProvider
        };
        private ApplicationConfiguration AppConfiguration = new ApplicationConfiguration();
        private ConfiguredEndpoint endpoint = new ConfiguredEndpoint();
        private Uri serverUrl = new Uri("opc.tcp://10.102.254.102:4863/");

        private static Opc.Ua.Client.ISession session;
        private static bool ConnectionInProgress = false;

        public OPCClient()
        {
            
        }

        public async Task Connect()
        {
            if (!ConnectionInProgress) { 
                ConnectionInProgress = true;
                try
                {
                    if (session == null || !session.Connected)
                    {
                        AppConfiguration = await AppInstance.LoadApplicationConfiguration(silent: false).ConfigureAwait(false);
                        AppConfiguration.SecurityConfiguration.SuppressNonceValidationErrors = true;

                        bool haveAppCertificate = await AppInstance.CheckApplicationInstanceCertificate(false, minimumKeySize: 0).ConfigureAwait(false);

                        if (!haveAppCertificate)
                        {
                            throw new Exception("Application instance certificate invalid!");
                        }

                        endpointDescription = CoreClientUtils.SelectEndpoint(AppConfiguration, serverUrl.ToString(), useSecurity);

                        EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(AppConfiguration);
                       // endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);
                        endpoint.Update(endpointConfiguration);
                        endpoint.Update(endpointDescription);

                        var sessionFactory = TraceableSessionFactory.Instance;
                        //Session OPC_session = new Session( , m_configuration, )
                        var _session = await sessionFactory.CreateAsync(
                                AppInstance.ApplicationConfiguration,
                                connection,
                                endpoint,
                                false,
                                false,
                                AppConfiguration.ApplicationName,
                                SessionLifeTime,
                                null,//UserIdentity,
                                null,
                                ct
                            ).ConfigureAwait(false);
                        session = _session;
                        session.KeepAliveInterval = 5000;
                        session.DeleteSubscriptionsOnClose = false;
                        session.TransferSubscriptionsOnReconnect = true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("OPC Client session creation failed\n" + ex.Message);
                }
                finally
                {
                    ConnectionInProgress = false;
                }
            }
        }

        public bool OPC_Client_Connected()
        {
            if (session == null)
            {
                return false;
            }
            return session.Connected;
        }
        ~OPCClient()
        {

        }

        public bool OPC_Read_String(string key, out string value)
        {
            try
            {
                if (session != null && session.Connected)
                {
                    value = session.ReadValue(new NodeId(key)).ToString();
                    return true;
                }
                else
                {
                    value = string.Empty;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Read failed with key " + key);
                Console.WriteLine(ex.Message);
                if (ex.Message == "BadNotConnected" || ex.Message == "BadNotReadable")
                {
                    //do nothing
                } else 
                if (!ConnectionInProgress)
                {
                    ConnectionInProgress = true;
                    if (session != null)
                    {
                        try
                        {
                            Task t = Task.Run(() => session.ReconnectAsync());
                            t.Wait();
                        }
                        catch (Exception ex2)
                        {
                            Console.WriteLine("Reconnect failed: " + ex2.Message);
                        }                      
                    }
                    else
                    {
                        try
                        {
                            Task t = Task.Run(() => this.Connect());
                            t.Wait();
                        }
                        catch (Exception  ex3 ) 
                        {
                            Console.WriteLine("Reconnect failed: " + ex3.Message);
                        }
                    }
                    ConnectionInProgress = false;
                }
                value = string.Empty;
                return false;
            }
        }

        public bool OPC_Read_Float(string key,out float value)
        {
            try
            {
                if (session != null && session.Connected)
                {
                    value = float.Parse(session.ReadValue(new NodeId(key)).ToString());
                    return true;
                }
                else
                {
                    value = 0;
                    return false;
                }
            }
            catch(Exception ex){
                Console.WriteLine("Read failed with key " + key);
                Console.WriteLine(ex.Message);
                if (ex.Message == "BadNotConnected" || ex.Message == "BadNotReadable")
                {
                    //do nothing
                } else
                if (!ConnectionInProgress)
                {
                    ConnectionInProgress = true;
                    if (session != null)
                    {
                        try
                        {
                            Task t = Task.Run(() => session.ReconnectAsync());
                            t.Wait();
                        }
                        catch (Exception ex2)
                        {
                            Console.WriteLine("Reconnect failed: " + ex2.Message);
                        }
                    }
                    else
                    {
                        try
                        {
                            Task t = Task.Run(() => this.Connect());
                            t.Wait();
                        }
                        catch (Exception ex3)
                        {
                            Console.WriteLine("Reconnect failed: " + ex3.Message);
                        }
                    }
                    ConnectionInProgress = false;
                }
                value = 0;
                return false;
            }
        }

        public bool OPC_Read_Int(string key, out int value)
        {
            try
            {
                if (session != null && session.Connected)
                {
                    value = int.Parse(session.ReadValue(new NodeId(key)).ToString());
                    return true;
                }
                else
                {
                    value = 0;
                    return false;
                }
            }
            catch(Exception ex){
                Console.WriteLine("Read failed with key " + key);
                Console.WriteLine(ex.Message);
                if (ex.Message == "BadNotConnected" || ex.Message == "BadNotReadable")
                {
                    //do nothing
                } else
                if (!ConnectionInProgress)
                {
                    ConnectionInProgress = true;
                    if (session != null)
                    {
                        try
                        {
                            Task t = Task.Run(() => session.ReconnectAsync());
                            t.Wait();
                        }
                        catch (Exception ex2)
                        {
                            Console.WriteLine("Reconnect failed: " + ex2.Message);
                        }
                    }
                    else
                    {
                        try
                        {
                            Task t = Task.Run(() => this.Connect());
                            t.Wait();
                        }
                        catch (Exception ex3)
                        {
                            Console.WriteLine("Reconnect failed: " + ex3.Message);
                        }
                    }
                    ConnectionInProgress = false;
                }
                value = 0;
                return false;
            }
        }

               /* public bool OPC_Read_Bool(string key)
                {
                    return false;
                }*/

        public void OPC_Client_Disconnect()
        {

        }
    }
}
