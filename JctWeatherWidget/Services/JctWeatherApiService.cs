using JctWeatherWidget.Helpers;
using JctWeatherWidget.Models;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text.Json;

namespace JctWeatherWidget.Services
{
    public class JctWeatherApiService(HttpClient client) : IWeatherService
    {
        private static readonly string ApiKey = ReadApiKey();

        private const string BaseUrl = "https://api.weatherapi.com/v1/forecast.json";
        private const byte DaysAmount = 1;

        public async Task<JctWeatherData> GetWeatherFromCoordinatesAsync(double lat, double lon, CancellationToken ct = default)
        {
            var latStr = lat.ToString(CultureInfo.InvariantCulture);
            var lonStr = lon.ToString(CultureInfo.InvariantCulture);
            var url = $"{BaseUrl}?key={ApiKey}&q={latStr},{lonStr}&days={DaysAmount}&lang=ru&aqi=no";

            try
            {
                var response = await client.GetAsync(url, ct);
                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync(ct);
                    throw new HttpRequestException($"Ошибка API: {response.StatusCode} — {errorText}");
                }

                using var stream = await response.Content.ReadAsStreamAsync(ct);
                var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
                var root = doc.RootElement;

                // Обработка ошибок сервиса в JSON (invalid key?)
                if (root.TryGetProperty("error", out var error))
                {
                    var msg = error.GetProperty("message").GetString();
                    throw new InvalidOperationException($"WeatherAPI error: {msg}");
                }

                JsonElement location = root.GetProperty("location");
                JsonElement current = root.GetProperty("current");

                // Данные по прогнозу на день

                JsonElement forecastDay = root
                    .GetProperty("forecast")
                    .GetProperty("forecastday")[0];

                string windir = current.GetStringSafe("wind_dir", "—");

                return new JctWeatherData
                {
                    Location = location.GetStringSafe("name", "Неизвестно"),
                    TemperatureC = current.GetDoubleSafe("temp_c"),
                    Description = current.TryGetProperty("condition", out var condProp) && condProp.TryGetProperty("text", out var textProp) ? textProp.GetString() : "—",
                    IconUrl = current.GetProperty("condition").TryGetProperty("icon", out var iconProp) ? "https:" + iconProp.GetString() : null,

                    Humidity = current.GetInt32Safe("humidity"),
                    PressureMb = current.GetInt32Safe("pressure_mb", 1013),

                    ChanceOfRain = forecastDay.GetProperty("day").GetInt32Safe("daily_chance_of_rain"),
                    ChanceOfSnow = forecastDay.GetProperty("day").GetInt32Safe("daily_chance_of_snow"),

                    WindKph = current.GetDoubleSafe("wind_kph"),
                    WindDegree = current.GetInt32Safe("wind_degree"),
                    WindDir = current.GetStringSafe("wind_dir", "—"),

                    Sunrise = JctDataParseHelper.ParseSunPeriodTime(location.GetProperty("localtime"), forecastDay.GetProperty("astro").GetStringSafe("sunrise", "—")),

                    Sunset = JctDataParseHelper.ParseSunPeriodTime(location.GetProperty("localtime"), forecastDay.GetProperty("astro").GetStringSafe("sunset", "—"))
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Не удалось получить погоду по координатам ({lat}, {lon})", ex);
            }
        }

        private static string ReadApiKey(string filePath = "api.key")
        {
            if (!File.Exists(filePath))
                throw new InvalidOperationException($"Файл с API-ключом не найден: {Path.GetFullPath(filePath)}\n" +
                    "Создайте файл 'api.key' в папке запуска и поместите туда ключ от WeatherAPI.");

            var key = File.ReadAllText(filePath).Trim();
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("API-ключ пустой.");

            return key;
        }
    }
}
