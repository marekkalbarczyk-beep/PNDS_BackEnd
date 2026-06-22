using PNDS_BackEnd_Prod.OPC_Client;

namespace PNDS_BackEnd_Prod.Services
{
    public class J1BerthingData
    {
        public int J1Angle { get; set; } = 0;
        public float J1Laser_R_Speed { get; set; } = 0.0f;
        public float J1Laser_R_Distance { get; set; } = 0.0f;
        public float J1Laser_L_Speed { get; set; } = 0.0f;
        public float J1Laser_L_Distance { get; set; } = 0.0f;

        public bool J1Status { get; set; } = false;
    }

    public interface IJ1BerthingService
    {
        J1BerthingData GetCurrentData();
    }



    public class J1BerthingService : IJ1BerthingService, IDisposable
    {
        private IOPCClient _opcClient;
        private ILogger<J1BerthingService> _logger;

        private J1BerthingData _currentData = new();
        private readonly object _lock = new(); // Dla bezpieczeństwa wątkowego
        private readonly CancellationTokenSource _cts = new();
        private readonly List<string> _tags = new()
        {

            "ns=1;s=t|SERVER2::B01ANGLE/ANGLE.Angle",
            "ns=1;s=t|SERVER2::B01LASER_A/DISTANCE.DistReal_m",
            "ns=1;s=t|SERVER2::B01LASER_B/DISTANCE.DistReal_m",
            "ns=1;s=t|SERVER2::B01LASER_A/SPEED.Vsgn",
            "ns=1;s=t|SERVER2::B01LASER_B/SPEED.Vsgn"
        };

        // Licznik czasu
        private DateTime _lastRequestTime = DateTime.MinValue;
        private bool _isPollingActive = false;
        private bool _sleepMessage = false;
        private readonly TimeSpan _timeout = TimeSpan.FromMinutes(2);

        public J1BerthingService( IOPCClient oPC, ILogger<J1BerthingService> logger)
        {

            _logger = logger;
            _opcClient = oPC;
            _ = RefreshLoop();

          //  _logger.LogInformation("Creating J1 ShipDataReader");
        }

        public J1BerthingData GetCurrentData()
            {
            lock (_lock)
            {
                _lastRequestTime = DateTime.Now;
                _isPollingActive = true;
                _sleepMessage = false;
                // Zwracamy kopię, aby nikt "z zewnątrz" nie zmienił danych w serwisie
                return new J1BerthingData
                {
                    J1Angle = _currentData.J1Angle,
                    J1Laser_R_Speed = _currentData.J1Laser_R_Speed,
                    J1Laser_R_Distance = _currentData.J1Laser_R_Distance,
                    J1Laser_L_Speed = _currentData.J1Laser_L_Speed,
                    J1Laser_L_Distance = _currentData.J1Laser_L_Distance,
                    J1Status = _currentData.J1Status
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
                        _logger.LogInformation("Odczyt danych z OPC: J1BerthingData");
#endif
                        var results = await _opcClient.OPC_ReadMultiple(_tags);

                        lock (_lock)
                        {
                            if (results.Count == _tags.Count && results.All(r => r.Status))
                            {
                                _currentData.J1Angle = Convert.ToInt32(results[0].Value);
                                _currentData.J1Laser_L_Distance = Convert.ToSingle(results[1].Value);
                                _currentData.J1Laser_R_Distance = Convert.ToSingle(results[2].Value);
                                _currentData.J1Laser_L_Speed = Convert.ToSingle(results[3].Value); ;
                                _currentData.J1Laser_R_Speed = Convert.ToSingle(results[4].Value); ;
                                _currentData.J1Status = true;
                            }
                        }
                    }
                    else //if (_opcClient.OPC_Client_Connected())
                    {
                        lock (_lock) { _currentData.J1Status = false; }
                        await _opcClient.Connect();
                    }
                }
                else  //shouldPool
                {
                    if (!_sleepMessage)
                    {
                        _logger.LogInformation("OPC Polling is sleeping: J1BerthingService");
                        _sleepMessage = true;
                    }
                }
            }//while
        }

        public void Dispose() => _cts.Cancel();

    }
}