using System.Text.Json;

namespace JctWeatherWidget.Helpers;

public static class JctDataParseHelper
{
    public static int GetInt32Safe(this JsonElement element, string propertyName, int fallback = 0)
    {
        return element.TryGetProperty(propertyName, out var prop)
            ? prop.GetInt32Safe(fallback)
            : fallback;
    }

    public static double GetDoubleSafe(this JsonElement element, string propertyName, double fallback = 0.0)
    {
        return element.TryGetProperty(propertyName, out var prop)
            ? prop.GetDoubleSafe(fallback)
            : fallback;
    }

    public static string GetStringSafe(this JsonElement element, string propertyName, string fallback = "—")
    {
        return element.TryGetProperty(propertyName, out var prop)
            ? prop.GetStringSafe(fallback)
            : fallback;
    }

    public static int GetInt32Safe(this JsonElement element, int fallback = 0)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Number => element.TryGetInt32(out var i) ? i : fallback,
            JsonValueKind.String => int.TryParse(element.GetString(), out var i) ? i : fallback,
            JsonValueKind.Null or JsonValueKind.Undefined => fallback,
            _ => element.TryGetDouble(out var d) ? (int)d : fallback
        };
    }

    public static double GetDoubleSafe(this JsonElement element, double fallback = 0.0)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Number => element.TryGetDouble(out var d) ? d : fallback,
            JsonValueKind.String => double.TryParse(element.GetString(), out var d) ? d : fallback,
            JsonValueKind.Null or JsonValueKind.Undefined => fallback,
            _ => fallback
        };
    }

    public static string GetStringSafe(this JsonElement element, string fallback = "—")
    {
        return element.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => fallback,
            JsonValueKind.String => element.GetString() ?? fallback,
            _ => element.ToString() ?? fallback
        };
    }

    public static DateTime ParseSunPeriodTime(JsonElement localTimeElement, string timeStr)
    {
        if (string.IsNullOrEmpty(timeStr))
            return DateTime.MinValue;

        // Парсим AM/PM
        if (DateTime.TryParseExact(timeStr, "hh:mm tt",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out var parsedTime))
        {
            var localTimeString = localTimeElement.GetString();
            if (DateTime.TryParse(localTimeString, out var localNow))
            {
                var result = localNow.Date + parsedTime.TimeOfDay;

                // Если восход "вечером" — завтра
                if (result < localNow && timeStr.Contains("AM"))
                    result = result.AddDays(1);

                // Если закат "утром" — сегодня
                if (result < localNow && timeStr.Contains("PM"))
                    result = result.AddDays(1);

                return result;
            }
        }

        return DateTime.MinValue;
    }
}
