using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing.Constraints;
using Newtonsoft.Json.Linq;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using PNDS_BackEnd_Dev.Services;
using Serilog;
using Serilog.Events;


namespace PNDS_BackEnd_Dev.OPC_Client
{

    public readonly record struct OpcResult<T>(bool Status, T? Value);


    public interface IOPCClient
    {
        Task Connect();
        bool OPC_Client_Connected();
        OpcResult<T> OPC_Read<T>(string key);
        //void OPC_Client_Disconnect();
    }

    public class OPCClient : IOPCClient
    {

        private readonly ILogger<OPCClient> _logger;
       // private readonly IConfiguration _config;

        private string OPC_Uri = String.Empty;


        private static string? password = null;
        private EndpointDescription? endpointDescription = null;
        private readonly bool useSecurity = false;
        
        private static string applicationName = "PNDS_OPC_Client";
        private static string configSectionName = "PNDS.OPC_Client";

        private ITransportWaitingConnection? connection = null;
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
        private ApplicationConfiguration AppConfiguration = new();
        private ConfiguredEndpoint endpoint = new();
        //private Uri serverUrl= new Uri("opc.tcp://10.102.254.102:4863/");
        private Uri serverUrl = new Uri("opc.tcp://10.102.36.100:48010/");

        private static Opc.Ua.Client.ISession? session;
        private static bool ConnectionInProgress = false;

        //private static readonly Serilog.ILogger _logger = Log.ForContext(typeof(OPCClient));

        //private Opc.Ua.Client.ISession? session;
        //private bool ConnectionInProgress = false;


        //public getOPCURI_from_configiration(IConfiguration configuration)
        //{
        //    // Odczytujemy wartość z sekcji UserSettings:JsonFilePath
        //    // Jeśli nie zostanie znaleziona, domyślnie używamy "users.json"
        //    serverUrl = new Uri(configuration["OPCSources:PNDSOPCSSource"]) ?? new Uri("opc.tcp://10.102.254.102:4863/");
        //}

        public OPCClient(ILogger<OPCClient> logger)
        {

            
            //_config = config;
            _logger = logger;

           _logger.LogInformation ("Create OPC Client instance");
        }

        public async Task Connect()
        {
            if (!ConnectionInProgress) { 
                ConnectionInProgress = true;
                _logger.LogInformation("Checking OPC session");
                try
                {    
                    if (session == null || !session.Connected)
                    {
                        _logger.LogInformation("Trying to establish OPC session");
                        AppConfiguration = await AppInstance.LoadApplicationConfiguration(silent: false).ConfigureAwait(false);
                        AppConfiguration.SecurityConfiguration.SuppressNonceValidationErrors = true;

                        bool haveAppCertificate = await AppInstance.CheckApplicationInstanceCertificates(silent: false).ConfigureAwait(false);

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
                        _logger.LogInformation("OPC session established");
                        _logger.LogInformation(session.Endpoint.EndpointUrl);
                        _logger.LogInformation(session.Endpoint.SecurityPolicyUri);


                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("OPC Client session creation failed\n" + ex.Message);
                    
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

        public OpcResult<T> OPC_Read<T>(string key)
        {
            try
            {
                if (session != null && session.Connected)
                {
                    var rawValue = session.ReadValue(new NodeId(key)).Value;
                    if (rawValue != null)
                    {
                        T convertedValue = (T)Convert.ChangeType(rawValue, typeof(T));
                        // Używamy zwięzłego konstruktora recordu
                        return new OpcResult<T>(true, convertedValue);
                    }                
                    return new OpcResult<T>(false, default);
                }
                else
                {
                    return new OpcResult<T>(false, default);
                }
            }
            catch (Exception ex)
            {
                 _logger.LogWarning(" OPC Read failed with key " + key);
                _logger.LogWarning(ex.Message);
                if (ex.Message == "BadNotReadable")
                {
                    //do nothing
                    return new OpcResult<T>(false, default);
                }
                else if (ex.Message == "BadSessionIdInvalid")
                {
                    if (session != null)
                    {
                        session.Dispose();
                        _logger.LogWarning("Disposing OPCClient session");
                    }
                    return new OpcResult<T>(false, default);
                }
                else
                {
                    if (!ConnectionInProgress)
                    {
                        ConnectionInProgress = true;
                        if (session != null)
                        {
                            try
                            {
                                _logger.LogInformation("OPCClient trying to reconnect");
                                Task t = Task.Run(() => session.ReconnectAsync());
                                t.Wait();
                            }
                            catch (Exception ex2)
                            {
                                _logger.LogError(" Reconnect failed: " + ex2.Message);
                            }
                        }
                        else
                        {
                            try
                            {
                                _logger.LogInformation("OPCClient trying to create new session");
                                Task t = Task.Run(() => this.Connect());
                                t.Wait();
                            }
                            catch (Exception ex3)
                            {
                                _logger.LogError(" Reconnect failed: " + ex3.Message);
                            }
                        }
                        ConnectionInProgress = false;
                    }
                    return new OpcResult<T>(false, default);
                }
            }
        }

        public void OPC_Client_Disconnect()
        {

        }
    }
}
