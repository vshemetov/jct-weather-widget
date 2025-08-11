using JctWeatherWidget.Models;

namespace JctWeatherWidget.Services;

public interface IWeatherService
{
    Task<JctWeatherData> GetWeatherFromCoordinatesAsync(double lat, double lon, CancellationToken ct = default);
}
