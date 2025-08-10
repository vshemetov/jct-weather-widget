using System.Text.Json;

namespace JctWeatherWidget.Helpers;

public static class JctDataParseHelper
{
    // Коэффициент: гПа → мм рт.ст.
    private const double HpaToMmHgFactor = 0.750062;

    /// <summary>
    /// Безопасно извлекает double из JsonElement (поддерживает число, строку с числом, null)
    /// </summary>
    public static bool TryGetDouble(this JsonElement element, out double value)
    {
        value = 0;

        return element.ValueKind switch
        {
            JsonValueKind.Number => element.TryGetDouble(out value),
            JsonValueKind.String => double.TryParse(element.GetString(), out value),
            JsonValueKind.Null or JsonValueKind.Undefined => false,
            _ => double.TryParse(element.GetRawText(), out value)
        };
    }

    /// <summary>
    /// Безопасно получает давление в мм рт.ст. из значения pressure_mb (в гПа)
    /// </summary>
    public static int GetPressureInMmHg(JsonElement element)
    {
        if (!TryGetDouble(element, out var hpa))
            return 760; // значение по умолчанию

        if (hpa <= 0)
            return 760;

        return (int)(hpa * HpaToMmHgFactor);
    }

    /// <summary>
    /// Безопасно получает строку, возвращая fallback при null или пусто
    /// </summary>
    public static string GetString(this JsonElement element, string fallback = "—")
    {
        return element.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => fallback,
            JsonValueKind.String => element.GetString() ?? fallback,
            _ => element.ToString() ?? fallback
        };
    }

    /// <summary>
    /// Безопасно получает int, возвращая fallback при ошибках
    /// </summary>
    public static int GetInt32(this JsonElement element, int fallback = 0)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Number => element.TryGetInt32(out var i) ? i : fallback,
            JsonValueKind.String => int.TryParse(element.GetString(), out var i) ? i : fallback,
            JsonValueKind.Null or JsonValueKind.Undefined => fallback,
            _ => fallback
        };
    }

    /// <summary>
    /// Безопасно получает double, возвращая fallback при ошибках
    /// </summary>
    public static double GetDouble(this JsonElement element, double fallback = 0.0)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Number => element.TryGetDouble(out var d) ? d : fallback,
            JsonValueKind.String => double.TryParse(element.GetString(), out var d) ? d : fallback,
            JsonValueKind.Null or JsonValueKind.Undefined => fallback,
            _ => fallback
        };
    }
}
