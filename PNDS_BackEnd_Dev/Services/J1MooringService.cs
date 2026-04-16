using System.ComponentModel.DataAnnotations;
using PNDS_BackEnd_Dev.OPC_Client;

namespace PNDS_BackEnd_Dev.Services
{
    public class J1MooringData
    {
        public int id { get; set; } = -1;
        public string j1Name { get; set; } = "";
        public int j1NoOfHooks { get; set; } = 0;
        public string j1StatusMsg { get; set; } = "";
        public float[] j1Values { get; set; } = Array.Empty<float>();
        public float j1UnitLoad { get; set; }
        public bool status { get; set; }
    }

    public interface IJ1MooringListService
    {
        IEnumerable<J1MooringData> GetCurrentData();
        J1MooringData? GetCurrentData(int id);
    }


    public class J1MooringListService : IJ1MooringListService, IDisposable
    {

        private readonly List<J1MooringService> _mooringServices = new();


        private readonly object _lock = new(); // Dla bezpieczeństwa wątkowego
        private readonly CancellationTokenSource _cts = new();

        public J1MooringListService(IOPCClient opcClient, ILoggerFactory loggerFactory)
        {
            _mooringServices.Add(new J1MooringService(1, "MD1", 3, false, opcClient, loggerFactory.CreateLogger<J1MooringService>()));
            _mooringServices.Add(new J1MooringService(2, "MD2", 2, false, opcClient, loggerFactory.CreateLogger<J1MooringService>()));
            _mooringServices.Add(new J1MooringService(3, "MD3", 2, false, opcClient, loggerFactory.CreateLogger<J1MooringService>()));
            _mooringServices.Add(new J1MooringService(4, "BD1", 2, false, opcClient, loggerFactory.CreateLogger<J1MooringService>()));
            _mooringServices.Add(new J1MooringService(5, "BD2", 2, false, opcClient, loggerFactory.CreateLogger<J1MooringService>()));
            _mooringServices.Add(new J1MooringService(6, "BD3", 2, false, opcClient, loggerFactory.CreateLogger<J1MooringService>()));
            _mooringServices.Add(new J1MooringService(7, "BD4", 2, false, opcClient, loggerFactory.CreateLogger<J1MooringService>()));
            _mooringServices.Add(new J1MooringService(8, "MD4", 2, false, opcClient, loggerFactory.CreateLogger<J1MooringService>()));
            _mooringServices.Add(new J1MooringService(9, "MD5", 2, false, opcClient, loggerFactory.CreateLogger<J1MooringService>()));
            _mooringServices.Add(new J1MooringService(10, "MD6", 3, false, opcClient, loggerFactory.CreateLogger<J1MooringService>()));
        }



        public IEnumerable<J1MooringData> GetCurrentData() 
            => _mooringServices.Select(s => s.GetData());

        public J1MooringData? GetCurrentData(int id)
            => _mooringServices.FirstOrDefault(s => s.GetData().id == id)?.GetData();

        public void Dispose()
        {
            foreach (var s in _mooringServices) s.Stop();
        }
    }


    public class J1MooringService 
    {
        private readonly IOPCClient _opcClient;
        private readonly ILogger<J1MooringService> _logger;
        private readonly J1MooringData _currentData;
        private readonly object _lock = new();
        private readonly CancellationTokenSource _cts = new();

        private DateTime _lastRequestTime = DateTime.MinValue;
        private readonly TimeSpan _timeout = TimeSpan.FromMinutes(3);
        private bool _isPollingActive = false;
        private bool _sleepMessage = false;

        private readonly Dictionary<int, string> _dalbTagsbMap = new()
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

        private readonly List<string> _tags = new();



        public J1MooringService( int id, string name, int noOfHooks, bool status, IOPCClient oPC, ILogger<J1MooringService> logger)
        {

            _logger = logger;
            _opcClient = oPC;

            _currentData = new J1MooringData
            {
                id = id,
                j1Name = name,
                j1NoOfHooks = noOfHooks,
                j1Values = new float[noOfHooks]
            };

            for (int i = 1; i <= noOfHooks; i++)
            {
                _tags.Add($"{_dalbTagsbMap[id]}H{i}_Ton");
            }
            _tags.Add($"{_dalbTagsbMap[id]}UnitLoad");


            _ = RefreshLoop();

          //  _logger.LogInformation("Creating J1 ShipDataReader");
        }


        public J1MooringData GetData()
        {
            lock (_lock)
            {
                _lastRequestTime = DateTime.Now;
                _isPollingActive = true;
                _sleepMessage = false;

                // Zwracamy kopię obiektu
                return new J1MooringData
                {
                    id = _currentData.id,
                    j1Name = _currentData.j1Name,
                    j1NoOfHooks = _currentData.j1NoOfHooks,
                    j1StatusMsg = _currentData.j1StatusMsg,
                    j1Values = (float[])_currentData.j1Values.Clone(),
                    j1UnitLoad = _currentData.j1UnitLoad,
                    status = _currentData.status
                };
            }
        }


        private async Task RefreshLoop()
        {
            var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1000));
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
                        _logger.LogInformation("Odczyt danych z OPC: J1MooringData " + _currentData.id.ToString());
#endif
                        var results = await _opcClient.OPC_ReadMultiple(_tags);

                        lock (_lock)
                        {
                            if (results.Count == _tags.Count && results.All(r => r.Status))
                            {
                                for (int i = 0; i < _currentData.j1NoOfHooks; i++)
                                {
                                    _currentData.j1Values[i] = (float)(Math.Ceiling(Convert.ToSingle(results[i].Value) * 10) / 10);
                                }
                                _currentData.j1UnitLoad = (float)(Math.Ceiling(Convert.ToSingle(results[_currentData.j1NoOfHooks].Value) * 10) / 10);

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
                        _logger.LogInformation("OPC Polling is sleeping: J1MooringData " + _currentData.id.ToString());
                        _sleepMessage = true;
                    }
                }
            }//while
        }

        public void Stop() => _cts.Cancel();

    }
}