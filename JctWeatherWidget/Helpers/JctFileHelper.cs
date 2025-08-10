using System.Globalization;
using System.IO;

namespace JctWeatherWidget.Helpers;

public static class JctFileHelper
{
    public static (double lat, double lon)? ReadCoordinates(string filePath = "location.txt")
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"❌ Файл не найден: {Path.GetFullPath(filePath)}");
            return null;
        }

        try
        {
            string line = File.ReadAllText(filePath).Trim();

            // Удаляем BOM, если есть (UTF-8)
            line = line.Trim(['\uFEFF', '\u200B']); // BOM, zero-width space

            // Токенизация с удалением лишних пробелов
            string[] tokens = line.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length != 2)
            {
                Console.WriteLine($"❌ Ожидаются 2 координаты, получено: {tokens.Length}");
                return null;
            }

            // Парсинг с разными CultureInfo
            static bool TryParseDouble(string s, out double result)
            {
                // Сначала пробуем с точкой (en-US)
                if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
                    return true;

                // Потом с запятой (ru-RU)
                if (double.TryParse(s, NumberStyles.Float, CultureInfo.GetCultureInfo("ru-RU"), out result))
                    return true;

                return false;
            }

            if (!TryParseDouble(tokens[0], out var lat))
            {
                Console.WriteLine($"❌ Не удалось распарсить широту: '{tokens[0]}'");
                return null;
            }

            if (!TryParseDouble(tokens[1], out var lon))
            {
                Console.WriteLine($"❌ Не удалось распарсить долготу: '{tokens[1]}'");
                return null;
            }

            // Проверка диапазонов гео. координат
            if (lat < -90 || lat > 90)
            {
                Console.WriteLine($"❌ Некорректная широта: {lat} (должна быть от -90 до 90)");
                return null;
            }

            if (lon < -180 || lon > 180)
            {
                Console.WriteLine($"❌ Некорректная долгота: {lon} (должна быть от -180 до 180)");
                return null;
            }

            Console.WriteLine($"✅ Координаты загружены: {lat:F6}, {lon:F6}");
            return (lat, lon);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка чтения файла: {ex.Message}");
            return null;
        }
    }
}
