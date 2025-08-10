namespace JctWeatherWidget.Models;

public class JctWeatherData
{
    public string Location { get; set; }
    public double TemperatureC { get; set; }
    public string Description { get; set; }
    public int Humidity { get; set; }
    public int Pressure { get; set; }

    public int? ChanceOfRain { get; set; }
    public int? ChanceOfSnow { get; set; }

    public string PrecipitationWarning
    {
        get
        {
            if (!ChanceOfRain.HasValue && !ChanceOfSnow.HasValue)
                return "";

            // Если только дождь
            if (ChanceOfRain.HasValue && !ChanceOfSnow.HasValue)
                return $"Дождь: {ChanceOfRain}%";

            // Если только снег
            if (ChanceOfSnow.HasValue && !ChanceOfRain.HasValue)
                return $"Снег: {ChanceOfSnow}%";

            // Оба есть — сравниваем
            if (ChanceOfRain > ChanceOfSnow)
                return $"Дождь: {ChanceOfRain}%";

            if (ChanceOfSnow > ChanceOfRain)
                return $"Снег: {ChanceOfSnow}%";

            // Равны — смотрим время года
            var now = DateTime.Now;
            var month = now.Month;

            // Зима: декабрь, январь, февраль
            bool isWinter = month == 12 || month == 1 || month == 2;

            return isWinter ? $"Снег: {ChanceOfSnow}%" : $"Дождь: {ChanceOfRain}%";
        }
    }

    public override string ToString() =>
            $"{Location}\n" +
            $"{(TemperatureC >= 0 ? "+" : "")}{TemperatureC:F0}°C  {Description}\n" +
            $"Вл: {Humidity}%  Давл: {Pressure} мм  {PrecipitationWarning}";
}
