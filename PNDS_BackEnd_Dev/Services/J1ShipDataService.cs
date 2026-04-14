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
                if (_opcClient.OPC_Client_Connected())
                {
                    // Odczyt danych z OPC
#if DEBUG
                    _logger.LogInformation("Odczyt danych z OPC: J1ShipDataService");
#endif
                    var tmpName = _opcClient.OPC_Read<String>("ns=1;s=t|SERVER2::AS1/ShipData1.ShipName");
                    var tmpDir = _opcClient.OPC_Read<int>("ns=1;s=t|SERVER2::AS1/ShipData1.HullDirection");

                    lock (_lock)
                    {
                        _currentData.J1ShipName = tmpName.Value ?? String.Empty;
                        _currentData.J1ShipDirection = tmpDir.Value;
                        _currentData.J1Status = tmpName.Status & tmpDir.Status;
                    }
                }
                else
                {
                    lock (_lock) { _currentData.J1Status = false; }
                    await _opcClient.Connect();
                }
            }
        }

        public void Dispose() => _cts.Cancel();

    }
}