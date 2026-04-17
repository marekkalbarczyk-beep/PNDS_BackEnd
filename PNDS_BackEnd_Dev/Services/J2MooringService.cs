using System.ComponentModel.DataAnnotations;
using PNDS_BackEnd_Dev.OPC_Client;

namespace PNDS_BackEnd_Dev.Services
{
    public class J2MooringData
    {
        public int id { get; set; } = -1;
        public string J2Name { get; set; } = "";
        public int J2NoOfHooks { get; set; } = 0;
        public string J2StatusMsg { get; set; } = "";
        public float[] J2Values { get; set; } = Array.Empty<float>();
        public float J2UnitLoad { get; set; }
        public bool status { get; set; }
    }

    public interface IJ2MooringListService
    {
        IEnumerable<J2MooringData> GetCurrentData();
        J2MooringData? GetCurrentData(int id);
    }


    public class J2MooringListService : IJ2MooringListService, IDisposable
    {

        private readonly List<J2MooringService> _mooringServices = new();


        private readonly object _lock = new(); // Dla bezpieczeństwa wątkowego
        private readonly CancellationTokenSource _cts = new();

        public J2MooringListService(IOPCClient opcClient, ILoggerFactory loggerFactory)
        {
            _mooringServices.Add(new J2MooringService(1, "MD1", 3, false, opcClient, loggerFactory.CreateLogger<J2MooringService>()));
            _mooringServices.Add(new J2MooringService(2, "MD2", 3, false, opcClient, loggerFactory.CreateLogger<J2MooringService>()));
            _mooringServices.Add(new J2MooringService(3, "MD3", 3, false, opcClient, loggerFactory.CreateLogger<J2MooringService>()));
            _mooringServices.Add(new J2MooringService(4, "MD4", 3, false, opcClient, loggerFactory.CreateLogger<J2MooringService>()));
            _mooringServices.Add(new J2MooringService(5, "MD5", 3, false, opcClient, loggerFactory.CreateLogger<J2MooringService>()));
            _mooringServices.Add(new J2MooringService(6, "BD1", 3, false, opcClient, loggerFactory.CreateLogger<J2MooringService>()));
            _mooringServices.Add(new J2MooringService(7, "BD2", 3, false, opcClient, loggerFactory.CreateLogger<J2MooringService>()));
            _mooringServices.Add(new J2MooringService(8, "BD3", 3, false, opcClient, loggerFactory.CreateLogger<J2MooringService>()));
            _mooringServices.Add(new J2MooringService(9, "BD4", 3, false, opcClient, loggerFactory.CreateLogger<J2MooringService>()));
            _mooringServices.Add(new J2MooringService(10, "BD5", 3, false, opcClient, loggerFactory.CreateLogger<J2MooringService>()));
            _mooringServices.Add(new J2MooringService(11, "MD6", 3, false, opcClient, loggerFactory.CreateLogger<J2MooringService>()));
            _mooringServices.Add(new J2MooringService(12, "MD7", 3, false, opcClient, loggerFactory.CreateLogger<J2MooringService>()));
            _mooringServices.Add(new J2MooringService(13, "MD8", 3, false, opcClient, loggerFactory.CreateLogger<J2MooringService>()));
            _mooringServices.Add(new J2MooringService(14, "MD9", 3, false, opcClient, loggerFactory.CreateLogger<J2MooringService>()));
        }



        public IEnumerable<J2MooringData> GetCurrentData() 
            => _mooringServices.Select(s => s.GetData());

        public J2MooringData? GetCurrentData(int id)
            => _mooringServices.FirstOrDefault(s => s.GetId() == id)?.GetData();

        public void Dispose()
        {
            foreach (var s in _mooringServices) s.Stop();
        }
    }


    public class J2MooringService 
    {
        private readonly IOPCClient _opcClient;
        private readonly ILogger<J2MooringService> _logger;
        private readonly J2MooringData _currentData;
        private readonly object _lock = new();
        private readonly CancellationTokenSource _cts = new();

        private DateTime _lastRequestTime = DateTime.MinValue;
        private readonly TimeSpan _timeout = TimeSpan.FromMinutes(3);
        private bool _isPollingActive = false;
        private bool _sleepMessage = false;

        private Random rnd = new();

        private static readonly Dictionary<int, string> _dalbTagsbMap = new()
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

        private readonly List<string> _tags = new();



        public J2MooringService( int id, string name, int noOfHooks, bool status, IOPCClient oPC, ILogger<J2MooringService> logger)
        {

            _logger = logger;
            _opcClient = oPC;

            _currentData = new J2MooringData
            {
                id = id,
                J2Name = name,
                J2NoOfHooks = noOfHooks,
                J2Values = new float[noOfHooks]
            };

            for (int i = 1; i <= noOfHooks; i++)
            {
                _tags.Add($"{_dalbTagsbMap[id]}H{i}_Ton");
            }
            _tags.Add($"{_dalbTagsbMap[id]}UnitLoad");


            _ = RefreshLoop();

          //  _logger.LogInformation("Creating J2 ShipDataReader");
        }

        public int GetId()
        {
            return _currentData.id;
        }


        public J2MooringData GetData()
        {
            lock (_lock)
            {
                _lastRequestTime = DateTime.Now;
                _isPollingActive = true;
                _sleepMessage = false;

                // Zwracamy kopię obiektu
                return new J2MooringData
                {
                    id = _currentData.id,
                    J2Name = _currentData.J2Name,
                    J2NoOfHooks = _currentData.J2NoOfHooks,
                    J2StatusMsg = _currentData.J2StatusMsg,
                    J2Values = (float[])_currentData.J2Values.Clone(),
                    J2UnitLoad = _currentData.J2UnitLoad,
                    status = _currentData.status
                };
            }
        }


        private async Task RefreshLoop()
        {
            var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(rnd.Next(800, 1300)));
            while (await timer.WaitForNextTickAsync(_cts.Token))
            {

                bool shouldPoll;
                lock (_lock)
                {
                    shouldPoll = (DateTime.Now - _lastRequestTime) < _timeout;
                    _isPollingActive = shouldPoll;
                }

                if (shouldPoll)
                {

                    if (_opcClient.OPC_Client_Connected())
                    {
                        // Odczyt danych z OPC
#if DEBUG
                        _logger.LogInformation("Odczyt danych z OPC: J2MooringData " + _currentData.id.ToString());
#endif
                        var results = await _opcClient.OPC_ReadMultiple(_tags);

                        lock (_lock)
                        {
                            if (results.Count == _tags.Count && results.All(r => r.Status))
                            {
                                for (int i = 0; i < _currentData.J2NoOfHooks; i++)
                                {
                                    _currentData.J2Values[i] = (float)(Math.Ceiling(Convert.ToSingle(results[i].Value) * 10) / 10);
                                }
                                _currentData.J2UnitLoad = (float)(Math.Ceiling(Convert.ToSingle(results[_currentData.J2NoOfHooks].Value) * 100) / 100);

                                _currentData.status = true;
                            }
                        }
                    }
                    else //if (_opcClient.OPC_Client_Connected())
                    {
                        lock (_lock) { _currentData.status = false; }
                        await _opcClient.Connect();
                    }
                }
                else  //shouldPool
                {
                    if (!_sleepMessage)
                    {
                        _logger.LogInformation("OPC Polling is sleeping: J2MooringData " + _currentData.id.ToString());
                        _sleepMessage = true;
                    }
                }
            }//while
        }

        public void Stop() => _cts.Cancel();

    }
}