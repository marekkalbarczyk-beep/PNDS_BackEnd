using PNDS_BackEnd_Prod.OPC_Client;

namespace PNDS_BackEnd_Prod.Services
{
    public class J2SeaStateData
    {
        public bool Status { get; set; } = false;
        public float J2CurrentSpeed { get; set; } = 0;
        public float J2CurrentDirection { get; set; } = 0;
        public float J2MeanWave { get; set; } = 0;
        public float J2MaxWave { get; set; } = 0;
        public float J2MeanPeriod { get; set; } = 0;
        public float J2Tide { get; set; } = 0;
    }

    public interface IJ2SeaStateService
    {
        J2SeaStateData GetCurrentData();
    }



    public class J2SeaStateService : IJ2SeaStateService, IDisposable
    {
        private IOPCClient _opcClient;
        private ILogger<J2SeaStateService> _logger;

        private J2SeaStateData _currentData = new();
        private readonly object _lock = new(); // Dla bezpieczeństwa wątkowego
        private readonly CancellationTokenSource _cts = new();
        private readonly List<string> _tags = new()
        {

            "ns=1;s=t|SERVER1::Current2/Current.Speed",
            "ns=1;s=t|SERVER1::Current2/Current.Direction",
            "ns=1;s=t|SERVER1::WaveTide2/MeanWave.Q_PV",
            "ns=1;s=t|SERVER1::WaveTide2/MaxWave.Q_PV",
            "ns=1;s=t|SERVER1::WaveTide2/MeanPeriod.Q_PV",
            "ns=1;s=t|SERVER1::WaveTide2/Tide.Q_PV"
        };

        // Licznik czasu
        private DateTime _lastRequestTime = DateTime.MinValue;
        private bool _isPollingActive = false;
        private bool _sleepMessage = false;
        private readonly TimeSpan _timeout = TimeSpan.FromMinutes(2);

        public J2SeaStateService( IOPCClient oPC, ILogger<J2SeaStateService> logger)
        {

            _logger = logger;
            _opcClient = oPC;
            _ = RefreshLoop();

          //  _logger.LogInformation("Creating J2 ShipDataReader");
        }

        public J2SeaStateData GetCurrentData()
            {
            lock (_lock)
            {
                _lastRequestTime = DateTime.Now;
                _isPollingActive = true;
                _sleepMessage = false;
                // Zwracamy kopię, aby nikt "z zewnątrz" nie zmienił danych w serwisie
                return new J2SeaStateData
                {
                    J2CurrentSpeed = _currentData.J2CurrentSpeed,
                    J2CurrentDirection = _currentData.J2CurrentDirection,
                    J2MeanWave = _currentData.J2MeanWave,
                    J2MaxWave = _currentData.J2MaxWave,
                    J2MeanPeriod = _currentData.J2MeanPeriod,
                    J2Tide = _currentData.J2Tide,
                    Status = _currentData.Status
                };
            }
        }

        private async Task RefreshLoop()
        {
            var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(2500));
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
                        _logger.LogInformation("Odczyt danych z OPC: J2SeaStateService");
#endif
                        var results = await _opcClient.OPC_ReadMultiple(_tags);

                        lock (_lock)
                        {
                            if (results.Count == _tags.Count && results.All(r => r.Status))
                            {
                                _currentData.J2CurrentSpeed = (float)(Math.Ceiling(Convert.ToSingle(results[0].Value) * 10) / 10);
                                _currentData.J2CurrentDirection = (float)(Math.Ceiling(Convert.ToSingle(results[1].Value) * 10) / 10);
                                _currentData.J2MeanWave = (float)(Math.Ceiling(Convert.ToSingle(results[2].Value) * 10) / 10);
                                _currentData.J2MaxWave = (float)(Math.Ceiling(Convert.ToSingle(results[3].Value) * 10) / 10);
                                _currentData.J2MeanPeriod = (float)(Math.Ceiling(Convert.ToSingle(results[4].Value) * 10) / 10);
                                _currentData.J2Tide = (float)(Math.Ceiling(Convert.ToSingle(results[5].Value) * 10) / 10);
                                _currentData.Status = true;
                            }
                        }
                    }
                    else //if (_opcClient.OPC_Client_Connected())
                    {
                        lock (_lock) { _currentData.Status = false; }
                        await _opcClient.Connect();
                    }
                }
                else  //shouldPool
                {
                    if (!_sleepMessage)
                    {
                        _logger.LogInformation("OPC Polling is sleeping: J2SeaStateService");
                        _sleepMessage = true;
                    }
                }
            }//while
        }

        public void Dispose() => _cts.Cancel();

    }
}