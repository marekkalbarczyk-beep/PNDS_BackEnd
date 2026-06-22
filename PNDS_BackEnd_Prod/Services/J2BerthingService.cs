using PNDS_BackEnd_Prod.OPC_Client;

namespace PNDS_BackEnd_Prod.Services
{
    public class J2BerthingData
    {
        public int J2Angle { get; set; } = 0;
        public float J2Laser_R_Speed { get; set; } = 0.0f;
        public float J2Laser_R_Distance { get; set; } = 0.0f;
        public float J2Laser_L_Speed { get; set; } = 0.0f;
        public float J2Laser_L_Distance { get; set; } = 0.0f;

        public bool J2Status { get; set; } = false;
    }

    public interface IJ2BerthingService
    {
        J2BerthingData GetCurrentData();
    }



    public class J2BerthingService : IJ2BerthingService, IDisposable
    {
        private IOPCClient _opcClient;
        private ILogger<J2BerthingService> _logger;

        private J2BerthingData _currentData = new();
        private readonly object _lock = new(); // Dla bezpieczeństwa wątkowego
        private readonly CancellationTokenSource _cts = new();

        // Licznik czasu
        private DateTime _lastRequestTime = DateTime.MinValue;
        private bool _isPollingActive = false;
        private bool _sleepMessage = false;
        private readonly TimeSpan _timeout = TimeSpan.FromMinutes(2);

        private readonly List<string> _tags = new()
        {

            "ns=1;s=t|SERVER1::B02ANGLE/ANGLE.Angle",
            "ns=1;s=t|SERVER1::B02LASER_RIGHT/DISTANCE.DistReal_m",
            "ns=1;s=t|SERVER1::B02LASER_LEFT/DISTANCE.DistReal_m",
            "ns=1;s=t|SERVER1::B02LASER_RIGHT/SPEED.Vsgn",
            "ns=1;s=t|SERVER1::B02LASER_LEFT/SPEED.Vsgn"
        };
        public J2BerthingService( IOPCClient oPC, ILogger<J2BerthingService> logger)
        {

            _logger = logger;
            _opcClient = oPC;
            _ = RefreshLoop();

          //  _logger.LogInformation("Creating J2 ShipDataReader");
        }

        public J2BerthingData GetCurrentData()
            {
            lock (_lock)
            {
                _lastRequestTime = DateTime.Now;
                _isPollingActive = true;
                _sleepMessage = false;
                // Zwracamy kopię, aby nikt "z zewnątrz" nie zmienił danych w serwisie
                return new J2BerthingData
                {
                    J2Angle = _currentData.J2Angle,
                    J2Laser_R_Speed = _currentData.J2Laser_R_Speed,
                    J2Laser_R_Distance = _currentData.J2Laser_R_Distance,
                    J2Laser_L_Speed = _currentData.J2Laser_L_Speed,
                    J2Laser_L_Distance = _currentData.J2Laser_L_Distance,
                    J2Status = _currentData.J2Status
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
                        _logger.LogInformation("Odczyt danych z OPC: J2BerthingData");
#endif
                        var results = await _opcClient.OPC_ReadMultiple(_tags);

                        lock (_lock)
                        {
                            if (results.Count == _tags.Count && results.All(r => r.Status))
                            {
                                _currentData.J2Angle = Convert.ToInt32(results[0].Value);
                                _currentData.J2Laser_R_Distance = Convert.ToSingle(results[1].Value);
                                _currentData.J2Laser_L_Distance = Convert.ToSingle(results[2].Value);
                                _currentData.J2Laser_R_Speed = Convert.ToSingle(results[3].Value); ;
                                _currentData.J2Laser_L_Speed = Convert.ToSingle(results[4].Value); ;
                                _currentData.J2Status = true;
                            }
                        }
                    }
                    else
                    {
                        lock (_lock) { _currentData.J2Status = false; }
                        await _opcClient.Connect();
                    }
                }
                else  //shouldPool
                {
                    if (!_sleepMessage)
                    {
                        _logger.LogInformation("OPC Polling is sleeping: J2BerthingService");
                        _sleepMessage = true;
                    }
                }
            }//while
        }

        public void Dispose() => _cts.Cancel();

    }
}