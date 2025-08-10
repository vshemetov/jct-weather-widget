using JctWeatherWidget.Helpers;
using JctWeatherWidget.Models;
using JctWeatherWidget.Services;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace JctWeatherWidget
{
    public partial class MainWindow : Window
    {
        private const int MaxRetryAttempts = 5;
        private const int RetryDelayMs = 10_000; // 10 секунд
        private static readonly TimeSpan UpdateInterval = TimeSpan.FromHours(1);

        private readonly JctWeatherService _weatherService;
        private DispatcherTimer _updateTimer;

        public MainWindow()
        {
            InitializeComponent();
            _weatherService = (App.Current as App).GetService<JctWeatherService>();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadWeatherWithRetry();
        }

        public async Task LoadWeatherWithRetry(int attempts = 0)
        {
            try
            {
                var coords = JctFileHelper.ReadCoordinates();
                if (!coords.HasValue)
                    throw new InvalidOperationException("Файл location.txt не найден или содержит неверные данные.");

                var (lat, lon) = coords.Value;
                var weather = await _weatherService.GetWeatherFromCoordinatesAsync(lat, lon);

                UpdateUI(weather);
                StartTimer();
            }
            catch (Exception ex) when (attempts < MaxRetryAttempts)
            {
                Console.WriteLine($"Ошибка ({attempts + 1}/{MaxRetryAttempts}): {ex.Message}\nПовтор через 10 сек...", "Jct Weather", MessageBoxButton.OK, MessageBoxImage.Warning);
                await Task.Delay(RetryDelayMs);
                await LoadWeatherWithRetry(attempts + 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось загрузить погоду: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateUI(JctWeatherData w)
        {
            LocationBlock.Text = w.Location;
            TempBlock.Text = $"{(w.TemperatureC >= 0 ? "+" : "")}{w.TemperatureC:F0}°C  {w.Description}";
            DetailsBlock.Text = $"Вл: {w.Humidity}%  Давл: {w.Pressure} мм {w.PrecipitationWarning}";
        }

        private void StartTimer()
        {
            _updateTimer = new DispatcherTimer
            {
                Interval = UpdateInterval
            };
            _updateTimer.Tick += async (s, e) => await LoadWeatherWithRetry();
            _updateTimer.Start();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}