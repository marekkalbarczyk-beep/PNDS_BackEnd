using PNDS_BackEnd_Prod.OPC_Client;

namespace PNDS_BackEnd_Prod.Services
{
    public class J1WeatherData
    {

        public bool Status { get; set; } = false;
        public float J1WD { get; set; } = 0; //Wind Direction
        public float J1WS { get; set; } = 0; //Wind Speed
        public float J1P { get; set; } = 0; // Preasure
        public float J1T { get; set; } = 0; //Temperature
        public float J1H { get; set; } = 0; //Humanidy
        public float J1R { get; set; } = 0; //Rain
    }

    public interface IJ1WeatherService
    {
        J1WeatherData GetCurrentData();
    }



    public class J1WeatherService : IJ1WeatherService, IDisposable
    {
        private IOPCClient _opcClient;
        private ILogger<J1WeatherService> _logger;

        private J1WeatherData _currentData = new();
        private readonly object _lock = new(); // Dla bezpieczeństwa wątkowego
        private readonly CancellationTokenSource _cts = new();
        private readonly List<string> _tags = new()
        {

            "ns=1;s=t|SERVER2::Wind1/WIND_COMPASS.Direction",
            "ns=1;s=t|SERVER2::Wind1/WIND_COMPASS.Speed",
            "ns=1;s=t|SERVER2::AtmPressure1/MEASURE.Q_PV",
            "ns=1;s=t|SERVER2::AirTemperature1/MEASURE.Q_PV",
            "ns=1;s=t|SERVER2::Humidity1/MEASURE.Q_PV",
            "ns=1;s=t|SERVER2::Rain1/MEASURE.Q_PV"
        };

        // Licznik czasu
        private DateTime _lastRequestTime = DateTime.MinValue;
        private bool _isPollingActive = false;
        private bool _sleepMessage = false;
        private readonly TimeSpan _timeout = TimeSpan.FromMinutes(2);

        public J1WeatherService( IOPCClient oPC, ILogger<J1WeatherService> logger)
        {

            _logger = logger;
            _opcClient = oPC;
            _ = RefreshLoop();

          //  _logger.LogInformation("Creating J1 ShipDataReader");
        }

        public J1WeatherData GetCurrentData()
            {
            lock (_lock)
            {
                _lastRequestTime = DateTime.Now;
                _isPollingActive = true;
                _sleepMessage = false;
                // Zwracamy kopię, aby nikt "z zewnątrz" nie zmienił danych w serwisie
                return new J1WeatherData
                {
                    J1WD = _currentData.J1WD,
                    J1WS = _currentData.J1WS,
                    J1P = _currentData.J1P,
                    J1T = _currentData.J1T,
                    J1H = _currentData.J1H,
                    J1R = _currentData.J1R,
                    Status = _currentData.Status
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
                        _logger.LogInformation("Odczyt danych z OPC: J1WeatherService");
#endif
                        var results = await _opcClient.OPC_ReadMultiple(_tags);

                        lock (_lock)
                        {
                            if (results.Count == _tags.Count && results.All(r => r.Status))
                            {
                                _currentData.J1WD = (float)(Math.Ceiling(Convert.ToSingle(results[0].Value) * 10) / 10);
                                _currentData.J1WS = (float)(Math.Ceiling(Convert.ToSingle(results[1].Value) * 10) / 10);
                                _currentData.J1P = (float)(Math.Ceiling(Convert.ToSingle(results[2].Value) * 10) / 10);
                                _currentData.J1T = (float)(Math.Ceiling(Convert.ToSingle(results[3].Value) * 10) / 10);
                                _currentData.J1H = (float)(Math.Ceiling(Convert.ToSingle(results[4].Value) * 10) / 10);
                                _currentData.J1R = (float)(Math.Ceiling(Convert.ToSingle(results[5].Value) * 10) / 10);
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
                        _logger.LogInformation("OPC Polling is sleeping: J1WeatherService");
                        _sleepMessage = true;
                    }
                }
            }//while
        }

        public void Dispose() => _cts.Cancel();

    }
}