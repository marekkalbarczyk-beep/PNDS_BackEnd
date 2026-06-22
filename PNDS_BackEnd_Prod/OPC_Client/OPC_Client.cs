using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing.Constraints;
using Newtonsoft.Json.Linq;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using PNDS_BackEnd_Prod.Services;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;


namespace PNDS_BackEnd_Prod.OPC_Client
{

    public readonly record struct OpcResult<T>(bool Status, T? Value);


    public interface IOPCClient
    {
        Task Connect();
        bool OPC_Client_Connected();
        Task<OpcResult<T>> OPC_Read<T>(string key);
        //void OPC_Client_Disconnect();
        Task<List<OpcResult<object>>> OPC_ReadMultiple(List<string> nodeIds);
    }

    public class SerilogTelemetryContext : ITelemetryContext
    {
        // 1. Logowanie (Microsoft.Extensions.Logging)
        public ILoggerFactory LoggerFactory { get; }

        // 2. Metryki (System.Diagnostics.Metrics)
        private readonly Meter _meter;

        // 3. Tracing (System.Diagnostics.ActivitySource)
        public ActivitySource ActivitySource { get; }

        public SerilogTelemetryContext()
        {
            // Łączymy fabrykę z Twoim statycznym Serilogiem
            LoggerFactory = new SerilogLoggerFactory(Log.Logger);

            // Tworzymy źródła dla metryk i śledzenia
            _meter = new Meter("Opc.Ua.Client");
            ActivitySource = new ActivitySource("Opc.Ua.Client");
        }

        // Implementacja metody Trace (wymagana przez interfejs)
        public void Trace(string message, params object[] args)
        {
            // Przekierowanie do Seriloga przez utworzoną fabrykę
            LoggerFactory.CreateLogger("OpcUa.Sdk").LogDebug(message, args);
        }

        // Implementacja metody CreateMeter (wymagana przez interfejs)
        public Meter CreateMeter()
        {
            return _meter;
        }
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

        private ITransportWaitingConnection connection = null!;
        private uint SessionLifeTime = 60 * 1000;
        //IUserIdentity UserIdentity = new UserIdentity(username, userpassword ?? string.Empty);
        private CancellationToken ct = default;

        // Define the UA Client application
        private static CertificatePasswordProvider PasswordProvider = new CertificatePasswordProvider(password);
        private static ITelemetryContext telemetry = new SerilogTelemetryContext();
        private ApplicationInstance AppInstance = new ApplicationInstance(telemetry)
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
                        AppConfiguration = await AppInstance.LoadApplicationConfigurationAsync(silent: false).ConfigureAwait(false);
                        AppConfiguration.SecurityConfiguration.SuppressNonceValidationErrors = true;

                        bool haveAppCertificate = await AppInstance.CheckApplicationInstanceCertificatesAsync(silent: false).ConfigureAwait(false);

                        if (!haveAppCertificate)
                        {
                            throw new Exception("Application instance certificate invalid!");
                        }

                        endpointDescription = await CoreClientUtils.SelectEndpointAsync(AppConfiguration, serverUrl.ToString(), useSecurity, telemetry);

                        EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(AppConfiguration);
                       // endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);
                        endpoint.Update(endpointConfiguration);
                        endpoint.Update(endpointDescription);

                        var sessionFactory = new DefaultSessionFactory(telemetry);
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

                        session.KeepAlive += (sender, e) =>
                        {
                            if (ServiceResult.IsBad(e.Status))
                            {
                                _logger.LogWarning("Problem z połączeniem: {0}. Próba odnowienia...", e.Status);
                                // Tutaj możesz ustawić flagę błędu dla Twoich serwisów
                            }
                            else
                            {
                                // To zdarzenie wyzwala się m.in. przy odświeżaniu tokenów
                                _logger.LogInformation("Sesja żyje. Stan: {0}", e.CurrentState);
                            }
                        };


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

        public async Task<OpcResult<T>> OPC_Read<T>(string nodeId)
        {
            try
            {
                if (session != null && session.Connected)
                {
                    DataValue rawValue = await session.ReadValueAsync(new NodeId(nodeId)).ConfigureAwait(false);
                    if (rawValue != null)
                    {
                        T convertedValue = (T)Convert.ChangeType(rawValue.Value, typeof(T));
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
            catch (ServiceResultException srex)
            {
                _logger.LogWarning("OPC Service Error: {Code}", srex.StatusCode);
                if (srex.StatusCode == Opc.Ua.StatusCodes.BadSessionIdInvalid || srex.StatusCode == Opc.Ua.StatusCodes.BadSessionClosed)
                {
                    session?.Dispose();
                    session = null;
                }
                return new OpcResult<T>(false, default);
            }
            catch (Exception ex)
            {
                 _logger.LogWarning(" OPC Read failed with key " + nodeId);
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
                                await session.ReconnectAsync();
                            }
                            catch (Exception ex2)
                            {
                                _logger.LogError(" Reconnect failed: " + ex2.Message);
                                session.Dispose();
                                session = null;
                            }
                        }
                        else
                        {
                            try
                            {
                                _logger.LogInformation("OPCClient trying to create new session");
                                await this.Connect();
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

        public async Task<List<OpcResult<object>>> OPC_ReadMultiple(List<string> nodeIds)
        {
            var results = new List<OpcResult<object>>();
            try
            {
                if (session == null || !session.Connected)
                    return nodeIds.Select(_ => new OpcResult<object>(false, null)).ToList();

                // 1. Tworzymy listę "życzeń" dla serwera
                ReadValueIdCollection nodesToRead = new ReadValueIdCollection();
                foreach (var id in nodeIds)
                {
                    nodesToRead.Add(new ReadValueId
                    {
                        NodeId = new NodeId(id),
                        AttributeId = Attributes.Value
                    });
                }

                // 2. Jeden zbiorczy odczyt
                var response = await session.ReadAsync(
                    null, 0, TimestampsToReturn.Both, nodesToRead, ct).ConfigureAwait(false);

                // 3. Przetworzenie wyników
                for (int i = 0; i < response.Results.Count; i++)
                {
                    var r = response.Results[i];
                    if (StatusCode.IsGood(r.StatusCode))
                    {
                        results.Add(new OpcResult<object>(true, r.Value));
                    }
                    else
                    {
                        results.Add(new OpcResult<object>(false, null));
                    }
                }
            }
            catch (ServiceResultException srex)
            {
                _logger.LogWarning("OPC Service Error: {Code}", srex.StatusCode);
                if (srex.StatusCode == Opc.Ua.StatusCodes.BadSessionIdInvalid || srex.StatusCode == Opc.Ua.StatusCodes.BadSessionClosed)
                {
                    session?.Dispose();
                    session = null;
                }
                results.Add(new OpcResult<object>(false, null));
            }
            catch (Exception ex)
            {
                _logger.LogError("Błąd odczytu grupowego: {Msg}", ex.Message);
                if (ex.Message == "BadNotReadable")
                {
                    //do nothing
                }
                else if (ex.Message == "BadSessionIdInvalid")
                {
                    if (session != null)
                    {
                        session.Dispose();
                        _logger.LogWarning("Disposing OPCClient session");
                    }
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
                                session.Dispose();
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
                }
            }
            return results;
        }

        public void OPC_Client_Disconnect()
        {

        }
    }
}
