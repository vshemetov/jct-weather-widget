namespace JctWeatherWidget.Models;

public class JctWeatherData
{
    public string Location { get; set; }
    public double TemperatureC { get; set; }
    public string Description { get; set; }
    public int Humidity { get; set; }
    public int PressureMb { get; set; }

    public string IconUrl { get; set; }

    public int? ChanceOfRain { get; set; } // // вероятность осадков, дождь
    public int? ChanceOfSnow { get; set; } // вероятность осадков, снег

    public double WindKph { get; set; }       // скорость ветра км/ч
    public int WindDegree { get; set; }    // направление
    public string WindDir { get; set; }    // аббревиатура: С, ЮЗ и т.д.
    public DateTime Sunrise { get; set; }  // восход
    public DateTime Sunset { get; set; }   // закат

    public int WindMps => (int)(WindKph / 3.6);
    public int PressureMmHg => (int)(PressureMb * 0.750062);
    public string WinDirRu => WindDir switch
    {
        "N" => "С",      // Север
        "NNE" => "ССВ",  // Север-северо-восток
        "NE" => "СВ",    // Северо-восток
        "ENE" => "ВСВ",  // Восток-северо-восток
        "E" => "В",      // Восток
        "ESE" => "ВЮВ",  // Восток-юго-восток
        "SE" => "ЮВ",    // Юго-восток
        "SSE" => "ЮЮВ",  // Юг-юго-восток
        "S" => "Ю",      // Юг
        "SSW" => "ЮЮЗ",  // Юг-юго-запад
        "SW" => "ЮЗ",    // Юго-запад
        "WSW" => "ЗЮЗ",  // Запад-юго-запад
        "W" => "З",      // Запад
        "WNW" => "ЗСЗ",  // Запад-северо-запад
        "NW" => "СЗ",    // Северо-запад
        "NNW" => "ССЗ",  // Север-северо-запад
        _ => WindDir
    };

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
            $"Вл: {Humidity}%  Давл: {PressureMmHg} мм  {PrecipitationWarning}" +
            $"Ветер: {WindMps} м/с ({WindDir})";
}
