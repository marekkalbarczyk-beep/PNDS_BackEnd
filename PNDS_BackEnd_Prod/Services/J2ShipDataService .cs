using PNDS_BackEnd_Prod.OPC_Client;

namespace PNDS_BackEnd_Prod.Services
{
    public class J2ShipData
    {
        public string J2ShipName { get; set; } = "";
        public int J2ShipDirection { get; set; } = -1;
        public bool J2Status { get; set; } = false;
    }

    public interface IJ2ShipService
    {
        J2ShipData GetCurrentData();
    }



    public class J2ShipService : IJ2ShipService, IDisposable
    {
        private IOPCClient _opcClient;
        private ILogger<J2ShipService> _logger;

        private J2ShipData _currentData = new();
        private readonly object _lock = new(); // Dla bezpieczeństwa wątkowego
        private readonly CancellationTokenSource _cts = new();

        // Licznik czasu
        private DateTime _lastRequestTime = DateTime.MinValue;
        private bool _isPollingActive = false;
        private bool _sleepMessage = false;
        private readonly TimeSpan _timeout = TimeSpan.FromMinutes(2);

        public J2ShipService( IOPCClient oPC, ILogger<J2ShipService> logger)
        {

            _logger = logger;
            _opcClient = oPC;
            _ = RefreshLoop();

          //  _logger.LogInformation("Creating J2 ShipDataReader");
        }

        public J2ShipData GetCurrentData()
            {
            lock (_lock)
            {
                _lastRequestTime = DateTime.Now;
                _isPollingActive = true;
                _sleepMessage = false;
                // Zwracamy kopię, aby nikt "z zewnątrz" nie zmienił danych w serwisie
                return new J2ShipData
                {
                    J2ShipName = _currentData.J2ShipName,
                    J2ShipDirection = _currentData.J2ShipDirection,
                    J2Status = _currentData.J2Status
                };
            }
        }

        private async Task RefreshLoop()
        {
            var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(5000));
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
                        _logger.LogInformation("Odczyt danych z OPC: J2ShipDataService");
#endif
                        var nameTask = _opcClient.OPC_Read<String>("ns=1;s=t|SERVER1::PLC/ShipData2.ShipName");
                        var dirTask = _opcClient.OPC_Read<int>("ns=1;s=t|SERVER1::PLC/ShipData2.HullDirection");

                        await Task.WhenAll(nameTask, dirTask);
                        var nameRes = await nameTask;
                        var dirRes = await dirTask;


                        lock (_lock)
                        {
                            _currentData.J2ShipName = nameRes.Value ?? String.Empty;
                            _currentData.J2ShipDirection = dirRes.Value;
                            _currentData.J2Status = nameRes.Status & dirRes.Status;
                        }
                    }
                    else
                    {
                        lock (_lock) { _currentData.J2Status = false; }
                        await _opcClient.Connect();
                    }//opcclientConnected
                }
                else  //shouldPool
                {
                    if (!_sleepMessage)
                    {
                        _logger.LogInformation("OPC Polling is sleeping: J2ShipService");
                        _sleepMessage = true;
                    }
                }
            }//while
        }

        public void Dispose() => _cts.Cancel();

    }
}