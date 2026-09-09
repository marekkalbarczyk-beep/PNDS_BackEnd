using PNDS_BackEnd_Prod.OPC_Client;

namespace PNDS_BackEnd_Prod.Services
{
    public class J1SeaStateData
    {
        public bool Status { get; set; } = default!;
        public float J1CurrentSpeed { get; set; } = 0;
        public float J1CurrentDirection { get; set; } = 0;
        public float J1MeanWave { get; set; } = 0;
        public float J1MaxWave { get; set; } = 0;
        public float J1MeanPeriod { get; set; } = 0;
        public float J1Tide { get; set; } = 0;
    }

    public interface IJ1SeaStateService
    {
        J1SeaStateData GetCurrentData();
    }



    public class J1SeaStateService : IJ1SeaStateService, IDisposable
    {
        private readonly IOPCClient _opcClient;
        private readonly ILogger<J1SeaStateService> _logger;
        private bool _disposed = false;

        private readonly J1SeaStateData _currentData = new();
        private readonly object _lock = new(); // Dla bezpieczeństwa wątkowego
        private readonly CancellationTokenSource _cts = new();
        private readonly List<string> _tags = new()
        {

            "ns=1;s=t|SERVER2::Current1/Current.Speed",
            "ns=1;s=t|SERVER2::Current1/Current.Direction",
            "ns=1;s=t|SERVER2::WaveTide1/MeanWave.Q_PV",
            "ns=1;s=t|SERVER2::WaveTide1/MaxWave.Q_PV",
            "ns=1;s=t|SERVER2::WaveTide1/MeanPeriod.Q_PV",
            "ns=1;s=t|SERVER2::WaveTide1/Tide.Q_PV"
        };

        // Licznik czasu
        private DateTime _lastRequestTime = DateTime.MinValue;
        private bool _sleepMessage = false;
        private readonly TimeSpan _timeout = TimeSpan.FromMinutes(2);

        public J1SeaStateService( IOPCClient oPC, ILogger<J1SeaStateService> logger)
        {

            _logger = logger;
            _opcClient = oPC;
            _ = RefreshLoop();
#if DEBUG
            _logger.LogInformation("Creating J1 ShipDataReader");
#endif
        }

        public J1SeaStateData GetCurrentData()
            {
            lock (_lock)
            {
                _lastRequestTime = DateTime.Now;
                _sleepMessage = false;
                // Zwracamy kopię, aby nikt "z zewnątrz" nie zmienił danych w serwisie
                return new J1SeaStateData
                {
                    J1CurrentSpeed = _currentData.J1CurrentSpeed,
                    J1CurrentDirection = _currentData.J1CurrentDirection,
                    J1MeanWave = _currentData.J1MeanWave,
                    J1MaxWave = _currentData.J1MaxWave,
                    J1MeanPeriod = _currentData.J1MeanPeriod,
                    J1Tide = _currentData.J1Tide,
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
                }

                if (shouldPoll)
                {

                    if (_opcClient.OPC_Client_Connected())
                    {
                        // Odczyt danych z OPC
#if DEBUG
                        _logger.LogInformation("Odczyt danych z OPC: J1SeaStateService");
#endif
                        var results = await _opcClient.OPC_ReadMultiple(_tags);

                        lock (_lock)
                        {
                            if (results.Count == _tags.Count && results.All(r => r.Status))
                            {
                                _currentData.J1CurrentSpeed = (float)(Math.Ceiling(Convert.ToSingle(results[0].Value) * 10) / 10);
                                _currentData.J1CurrentDirection = (float)(Math.Ceiling(Convert.ToSingle(results[1].Value) * 10) / 10);
                                _currentData.J1MeanWave = (float)(Math.Ceiling(Convert.ToSingle(results[2].Value) * 10) / 10);
                                _currentData.J1MaxWave = (float)(Math.Ceiling(Convert.ToSingle(results[3].Value) * 10) / 10);
                                _currentData.J1MeanPeriod = (float)(Math.Ceiling(Convert.ToSingle(results[4].Value) * 10) / 10);
                                _currentData.J1Tide = (float)(Math.Ceiling(Convert.ToSingle(results[5].Value) * 10) / 10);
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
                        _logger.LogInformation("OPC Polling is sleeping: J1SeaStateService");
                        _sleepMessage = true;
                    }
                }
            }//while
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                this._cts.Cancel();
                this._cts.Dispose();
            }
            _disposed = true;
        }

    }
}