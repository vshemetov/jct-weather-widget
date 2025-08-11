using JctWeatherWidget.Helpers;
using JctWeatherWidget.Models;
using JctWeatherWidget.Services;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace JctWeatherWidget
{
    public partial class MainWindow : Window
    {
        private const int MaxRetryAttempts = 5;
        private const int RetryDelayMs = 10_000; // 10 секунд
        private static readonly TimeSpan UpdateInterval = TimeSpan.FromMinutes(15);

        private readonly IWeatherService _weatherService;
        private DispatcherTimer _updateTimer;

        public MainWindow()
        {
            InitializeComponent();

            // Установка окна поверх рабочего стола, но не других окон
            Top = SystemParameters.WorkArea.Height - Height - 20;
            Left = SystemParameters.WorkArea.Width - Width - 20;

            // Специальные флаги окна
            var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            if (hwnd == IntPtr.Zero)
            {
                this.SourceInitialized += (s, e) =>
                {
                    hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                    SetWindowExTransparent(hwnd);
                };
            }
            else
            {
                SetWindowExTransparent(hwnd);
            }

            _weatherService = (App.Current as App).GetService<IWeatherService>();
        }

        private void SetWindowExTransparent(IntPtr hwnd)
        {
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TOOLWINDOW);
        }

        #region WinAPI
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
        #endregion

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
                JctWeatherData weather = await _weatherService.GetWeatherFromCoordinatesAsync(lat, lon);

                UpdateUI(weather);
                StartTimer();

                SetErrText(string.Empty);
            }
            catch (Exception ex) when (attempts < MaxRetryAttempts)
            {
                string errText = $"Ошибка ({attempts + 1}/{MaxRetryAttempts}): {ex.Message}\nПовтор через 10 сек...";
                SetErrText(errText);

                await Task.Delay(RetryDelayMs);
                await LoadWeatherWithRetry(attempts + 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось загрузить погоду: {ex.Message}");
                SetErrText(string.Empty);
            }
        }

        private void UpdateUI(JctWeatherData w)
        {
            LocationBlock.Text = w.Location;
            TempBlock.Text = $"{(w.TemperatureC >= 0 ? "+" : "")}{w.TemperatureC:F0}°C  {w.Description}";
            DetailsBlock.Text = $"Вл: {w.Humidity}%  Давл: {w.PressureMmHg} мм {w.PrecipitationWarning}";

            string windText = w.WindKph > 0 ? $"Ветер: {w.WindMps} м/с {w.WinDirRu}" : "Ветер: —";

            WindSunBlock.Text = $"Восх: {w.Sunrise:HH:mm}  Закат: {w.Sunset:HH:mm}  {windText}";
            WindSunBlock.Visibility = Visibility.Visible;

            UpdateDate();
            SetWeatherIcon(w.IconUrl);
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

        private void UpdateDate()
        {
            var now = DateTime.Now;
            var culture = new CultureInfo("ru-RU");

            // Получаем день недели: "пн", "вт"...
            var dayName = now.ToString("ddd", culture).ToLower();

            // Делаем первую букву заглавной
            var capitalizedDayName = dayName.Length > 0
                ? char.ToUpper(dayName[0]) + dayName[1..]
                : "??";

            // Форматируем дату: "10 авг"
            string dayMonth = now.ToString("d MMM", culture).Replace(".", "").Trim();
            DateBlock.Text = $"{capitalizedDayName}, {dayMonth}";
        }

        private void SetWeatherIcon(string iconUrl)
        {
            if (string.IsNullOrEmpty(iconUrl))
            {
                WeatherIcon.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(iconUrl);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                if (bitmap.IsDownloading)
                {
                    bitmap.DownloadCompleted += (sender, e) =>
                    {
                        WeatherIcon.Source = bitmap.ApplyGrayscale();
                        WeatherIcon.Visibility = Visibility.Visible;
                    };
                }
                else
                {
                    WeatherIcon.Source = bitmap.ApplyGrayscale();
                    WeatherIcon.Visibility = Visibility.Visible;
                }


            }
            catch(Exception ex)
            {
                WeatherIcon.Visibility = Visibility.Collapsed;
                SetErrText(ex.Message);
            }
        }

        private void SetErrText(string errText)
        {
            ErrorBlock.Text = errText;
            ErrorBlock.Visibility = string.IsNullOrEmpty(errText) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}