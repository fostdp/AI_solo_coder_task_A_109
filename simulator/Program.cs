using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SensorSimulator
{
    class Program
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string _apiBaseUrl = "http://localhost:5000/api/sensor-data";
        private const int _reportIntervalMinutes = 45;
        private static readonly Random _random = new Random();

        private static readonly List<SensorConfig> _ionSensors = new List<SensorConfig>();
        private static readonly List<SensorConfig> _envSensors = new List<SensorConfig>();
        private static readonly Dictionary<int, double> _sculptureBaseSalt = new Dictionary<int, double>();
        private static readonly Dictionary<int, double> _sculptureBaseCoverage = new Dictionary<int, double>();

        static async Task Main(string[] args)
        {
            Console.WriteLine("==============================================");
            Console.WriteLine("  古代泥塑彩绘层盐分迁移传感器 Wi-Fi 模拟器");
            Console.WriteLine("==============================================");
            Console.WriteLine($"上报间隔: {_reportIntervalMinutes} 分钟");
            Console.WriteLine($"API地址: {_apiBaseUrl}");
            Console.WriteLine("==============================================\n");

            InitializeSensors();

            var fastMode = args.Length > 0 && args[0] == "--fast";
            var singleShot = args.Length > 0 && args[0] == "--once";

            if (fastMode)
            {
                Console.WriteLine(">>> 快速模式: 每2秒上报一次数据");
                await RunFastMode();
            }
            else if (singleShot)
            {
                Console.WriteLine(">>> 单次上报模式");
                await ReportAllSensorsAsync();
                Console.WriteLine("\n单次上报完成！");
            }
            else
            {
                Console.WriteLine(">>> 正常模式: 每45分钟上报一次数据");
                Console.WriteLine(">>> 首次上报将在3秒后开始...\n");
                await RunNormalMode();
            }
        }

        private static void InitializeSensors()
        {
            var alertSculptures = new HashSet<int> { 6, 21 };
            var warningSculptures = new HashSet<int> { 3, 9, 15, 24, 28 };

            for (int i = 1; i <= 30; i++)
            {
                double baseSalt = 100;
                double baseCoverage = 5;

                if (alertSculptures.Contains(i))
                {
                    baseSalt = 550 + _random.NextDouble() * 150;
                    baseCoverage = 32 + _random.NextDouble() * 15;
                }
                else if (warningSculptures.Contains(i))
                {
                    baseSalt = 280 + _random.NextDouble() * 180;
                    baseCoverage = 18 + _random.NextDouble() * 12;
                }
                else
                {
                    baseSalt = 80 + _random.NextDouble() * 120;
                    baseCoverage = 3 + _random.NextDouble() * 12;
                }

                _sculptureBaseSalt[i] = baseSalt;
                _sculptureBaseCoverage[i] = baseCoverage;
            }

            int ionId = 1;
            for (int s = 1; s <= 30; s++)
            {
                int ionCount = s <= 21 ? 2 : 1;
                for (int i = 0; i < ionCount && ionId <= 40; i++)
                {
                    _ionSensors.Add(new SensorConfig
                    {
                        Id = ionId,
                        SensorCode = $"ION-{s:000}-{i + 1:00}",
                        SculptureId = s,
                        SensorType = "ion",
                        Model = "IonSensor-Pro-200",
                        DataType = "ion"
                    });
                    ionId++;
                }
            }

            for (int s = 1; s <= 30; s++)
            {
                _envSensors.Add(new SensorConfig
                {
                    Id = 40 + s,
                    SensorCode = $"ENV-{s:000}-01",
                    SculptureId = s,
                    SensorType = "environment",
                    Model = "EnvSensor-HT-500",
                    DataType = "environment"
                });
            }

            Console.WriteLine($"已初始化 {_ionSensors.Count} 台离子迁移传感器");
            Console.WriteLine($"已初始化 {_envSensors.Count} 台温湿度传感器");
            Console.WriteLine($"总计 {_ionSensors.Count + _envSensors.Count} 台传感器\n");
        }

        private static async Task RunNormalMode()
        {
            await Task.Delay(3000);
            while (true)
            {
                try
                {
                    await ReportAllSensorsAsync();
                    await Task.Delay(TimeSpan.FromMinutes(_reportIntervalMinutes));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 发生错误: {ex.Message}");
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            }
        }

        private static async Task RunFastMode()
        {
            while (true)
            {
                try
                {
                    await ReportAllSensorsAsync();
                    await Task.Delay(2000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 发生错误: {ex.Message}");
                    await Task.Delay(1000);
                }
            }
        }

        private static async Task ReportAllSensorsAsync()
        {
            var timestamp = DateTime.Now;
            Console.WriteLine($"[{timestamp:yyyy-MM-dd HH:mm:ss}] 开始上报传感器数据...");

            var allData = new List<SensorDataPayload>();
            int successCount = 0;
            int failCount = 0;

            foreach (var sensor in _ionSensors)
            {
                var data = GenerateIonSensorData(sensor, timestamp);
                allData.Add(data);
                
                try
                {
                    var result = await SendDataAsync(data);
                    if (result) successCount++;
                    else failCount++;
                }
                catch
                {
                    failCount++;
                }
                await Task.Delay(50);
            }

            foreach (var sensor in _envSensors)
            {
                var data = GenerateEnvSensorData(sensor, timestamp);
                allData.Add(data);
                
                try
                {
                    var result = await SendDataAsync(data);
                    if (result) successCount++;
                    else failCount++;
                }
                catch
                {
                    failCount++;
                }
                await Task.Delay(50);
            }

            Console.WriteLine($"[{timestamp:yyyy-MM-dd HH:mm:ss}] 上报完成: 成功 {successCount} 条, 失败 {failCount} 条");
        }

        private static SensorDataPayload GenerateIonSensorData(SensorConfig sensor, DateTime timestamp)
        {
            var baseSalt = _sculptureBaseSalt[sensor.SculptureId];
            var baseCoverage = _sculptureBaseCoverage[sensor.SculptureId];
            
            var hourFactor = 1 + 0.1 * Math.Sin((timestamp.Hour - 6) * Math.PI / 12);
            var randomFactor = 0.9 + _random.NextDouble() * 0.2;

            var naConcentration = baseSalt * 0.4 * hourFactor * randomFactor;
            var kConcentration = baseSalt * 0.25 * hourFactor * (0.95 + _random.NextDouble() * 0.1);
            var caConcentration = baseSalt * 0.35 * hourFactor * (0.9 + _random.NextDouble() * 0.2);

            return new SensorDataPayload
            {
                SensorId = sensor.Id,
                SensorCode = sensor.SensorCode,
                SculptureId = sensor.SculptureId,
                Timestamp = timestamp,
                DataType = sensor.DataType,
                SodiumIon = Math.Round(naConcentration, 2),
                PotassiumIon = Math.Round(kConcentration, 2),
                CalciumIon = Math.Round(caConcentration, 2),
                SaltConcentration = Math.Round(naConcentration + kConcentration + caConcentration, 2),
                CrystalCoverage = Math.Round(baseCoverage * (0.95 + _random.NextDouble() * 0.1), 2),
                SignalStrength = Math.Round(-50 - _random.NextDouble() * 30, 1),
                BatteryLevel = 75 + _random.Next(20)
            };
        }

        private static SensorDataPayload GenerateEnvSensorData(SensorConfig sensor, DateTime timestamp)
        {
            var baseTemp = 18.0;
            var baseHumidity = 65.0;
            
            var hourTemp = 6 * Math.Sin((timestamp.Hour - 6) * Math.PI / 12);
            var hourHumidity = -15 * Math.Sin((timestamp.Hour - 6) * Math.PI / 12);

            return new SensorDataPayload
            {
                SensorId = sensor.Id,
                SensorCode = sensor.SensorCode,
                SculptureId = sensor.SculptureId,
                Timestamp = timestamp,
                DataType = sensor.DataType,
                Temperature = Math.Round(baseTemp + hourTemp + (_random.NextDouble() - 0.5) * 2, 1),
                Humidity = Math.Round(baseHumidity + hourHumidity + (_random.NextDouble() - 0.5) * 10, 1),
                SignalStrength = Math.Round(-55 - _random.NextDouble() * 25, 1),
                BatteryLevel = 80 + _random.Next(18)
            };
        }

        private static async Task<bool> SendDataAsync(SensorDataPayload data)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                var json = JsonSerializer.Serialize(data, options);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_apiBaseUrl, content);
                
                if (response.IsSuccessStatusCode)
                {
                    Console.Write(".");
                    return true;
                }
                else
                {
                    Console.Write("x");
                    return false;
                }
            }
            catch (HttpRequestException)
            {
                Console.Write("!");
                return false;
            }
        }
    }

    public class SensorConfig
    {
        public int Id { get; set; }
        public string SensorCode { get; set; }
        public int SculptureId { get; set; }
        public string SensorType { get; set; }
        public string Model { get; set; }
        public string DataType { get; set; }
    }

    public class SensorDataPayload
    {
        public int SensorId { get; set; }
        public string SensorCode { get; set; }
        public int SculptureId { get; set; }
        public DateTime Timestamp { get; set; }
        public string DataType { get; set; }
        public double? SodiumIon { get; set; }
        public double? PotassiumIon { get; set; }
        public double? CalciumIon { get; set; }
        public double? SaltConcentration { get; set; }
        public double? CrystalCoverage { get; set; }
        public double? Temperature { get; set; }
        public double? Humidity { get; set; }
        public double SignalStrength { get; set; }
        public int BatteryLevel { get; set; }
    }
}
