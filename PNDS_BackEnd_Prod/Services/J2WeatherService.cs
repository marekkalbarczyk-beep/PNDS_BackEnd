using PNDS_BackEnd_Prod.OPC_Client;

namespace PNDS_BackEnd_Prod.Services
{
    public class J2WeatherData
    {

        public bool Status { get; set; } = false;
        public float J2WD { get; set; } = 0; //Wind Direction
        public float J2WS { get; set; } = 0; //Wind Speed
        public float J2P { get; set; } = 0; // Preasure
        public float J2T { get; set; } = 0; //Temperature
        public float J2H { get; set; } = 0; //Humanidy
        public float J2R { get; set; } = 0; //Rain
    }

    public interface IJ2WeatherService
    {
        J2WeatherData GetCurrentData();
    }



    public class J2WeatherService : IJ2WeatherService, IDisposable
    {
        private IOPCClient _opcClient;
        private ILogger<J2WeatherService> _logger;

        private J2WeatherData _currentData = new();
        private readonly object _lock = new(); // Dla bezpieczeństwa wątkowego
        private readonly CancellationTokenSource _cts = new();
        private readonly List<string> _tags = new()
        {

            "ns=1;s=t|SERVER1::Wind/WIND_COMPASS.Direction",
            "ns=1;s=t|SERVER1::Wind/WIND_COMPASS.Speed",
            "ns=1;s=t|SERVER1::AtmPressure/MEASURE.Q_PV",
            "ns=1;s=t|SERVER1::AirTemperature/MEASURE.Q_PV",
            "ns=1;s=t|SERVER1::Humidity/MEASURE.Q_P",
            "ns=1;s=t|SERVER1::Rain/MEASURE.Q_PV"
        };

        // Licznik czasu
        private DateTime _lastRequestTime = DateTime.MinValue;
        private bool _isPollingActive = false;
        private bool _sleepMessage = false;
        private readonly TimeSpan _timeout = TimeSpan.FromMinutes(2);

        public J2WeatherService( IOPCClient oPC, ILogger<J2WeatherService> logger)
        {

            _logger = logger;
            _opcClient = oPC;
            _ = RefreshLoop();

          //  _logger.LogInformation("Creating J2 ShipDataReader");
        }

        public J2WeatherData GetCurrentData()
            {
            lock (_lock)
            {
                _lastRequestTime = DateTime.Now;
                _isPollingActive = true;
                _sleepMessage = false;
                // Zwracamy kopię, aby nikt "z zewnątrz" nie zmienił danych w serwisie
                return new J2WeatherData
                {
                    J2WD = _currentData.J2WD,
                    J2WS = _currentData.J2WS,
                    J2P = _currentData.J2P,
                    J2T = _currentData.J2T,
                    J2H = _currentData.J2H,
                    J2R = _currentData.J2R,
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
                        _logger.LogInformation("Odczyt danych z OPC: J2WeatherService");
#endif
                        var results = await _opcClient.OPC_ReadMultiple(_tags);

                        lock (_lock)
                        {
                            if (results.Count == _tags.Count && results.All(r => r.Status))
                            {
                                _currentData.J2WD = (float)(Math.Ceiling(Convert.ToSingle(results[0].Value) * 10) / 10);
                                _currentData.J2WS = (float)(Math.Ceiling(Convert.ToSingle(results[1].Value) * 10) / 10);
                                _currentData.J2P = (float)(Math.Ceiling(Convert.ToSingle(results[2].Value) * 10) / 10);
                                _currentData.J2T = (float)(Math.Ceiling(Convert.ToSingle(results[3].Value) * 10) / 10);
                                _currentData.J2H = (float)(Math.Ceiling(Convert.ToSingle(results[4].Value) * 10) / 10);
                                _currentData.J2R = (float)(Math.Ceiling(Convert.ToSingle(results[5].Value) * 10) / 10);
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
                        _logger.LogInformation("OPC Polling is sleeping: J2WeatherService");
                        _sleepMessage = true;
                    }
                }
            }//while
        }

        public void Dispose() => _cts.Cancel();

    }
}