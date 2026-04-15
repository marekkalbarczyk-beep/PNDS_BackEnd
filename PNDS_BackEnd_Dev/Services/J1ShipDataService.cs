using PNDS_BackEnd_Dev.OPC_Client;

namespace PNDS_BackEnd_Dev.Services
{
    public class J1ShipData
    {
        public string J1ShipName { get; set; } = "";
        public int J1ShipDirection { get; set; } = -1;
        public bool J1Status { get; set; } = false;
    }

    public interface IJ1ShipService
    {
        J1ShipData GetCurrentData();
    }



    public class J1ShipService : IJ1ShipService, IDisposable
    {
        private IOPCClient _opcClient;
        private ILogger<J1ShipService> _logger;

        private J1ShipData _currentData = new();
        private readonly object _lock = new(); // Dla bezpieczeństwa wątkowego
        private readonly CancellationTokenSource _cts = new();

        // Licznik czasu
        private DateTime _lastRequestTime = DateTime.MinValue;
        private bool _isPollingActive = false;
        private bool _sleepMessage = false;
        private readonly TimeSpan _timeout = TimeSpan.FromMinutes(2);

        public J1ShipService( IOPCClient oPC, ILogger<J1ShipService> logger)
        {

            _logger = logger;
            _opcClient = oPC;
            _ = RefreshLoop();

          //  _logger.LogInformation("Creating J1 ShipDataReader");
        }

        public J1ShipData GetCurrentData()
            {
            lock (_lock)
            {
                _lastRequestTime = DateTime.Now;
                _isPollingActive = true;
                _sleepMessage = false;
                // Zwracamy kopię, aby nikt "z zewnątrz" nie zmienił danych w serwisie
                return new J1ShipData
                {
                    J1ShipName = _currentData.J1ShipName,
                    J1ShipDirection = _currentData.J1ShipDirection,
                    J1Status = _currentData.J1Status
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
                        _logger.LogInformation("Odczyt danych z OPC: J1ShipDataService");
#endif
                        var nameTask = _opcClient.OPC_Read<String>("ns=1;s=t|SERVER2::AS1/ShipData1.ShipName");
                        var dirTask = _opcClient.OPC_Read<int>("ns=1;s=t|SERVER2::AS1/ShipData1.HullDirection");

                        await Task.WhenAll(nameTask, dirTask);
                        var nameRes = await nameTask;
                        var dirRes = await dirTask;

                        lock (_lock)
                        {
                            _currentData.J1ShipName = nameRes.Value ?? String.Empty;
                            _currentData.J1ShipDirection = dirRes.Value;
                            _currentData.J1Status = nameRes.Status && dirRes.Status;
                        }
                    }
                    else
                    {
                        lock (_lock) { _currentData.J1Status = false; }
                        await _opcClient.Connect();
                    } //_opcClinetConnectoed
                }
                else  //shouldPool
                {
                    if (!_sleepMessage)
                    {
                        _logger.LogInformation("OPC Polling is sleeping: J1ShipService");
                        _sleepMessage = true;
                    }
                }
            }//while
        }

        public void Dispose() => _cts.Cancel();

    }
}