using Hardcodet.Wpf.TaskbarNotification;
using JctWeatherWidget.Log;
using JctWeatherWidget.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace JctWeatherWidget
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;
        private TaskbarIcon _notifyIcon;

        private IJctLogger _logger;

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection(); 
            ConfigureServices(services);

            // Билд провайдера после регистрации сервисов
            _serviceProvider = services.BuildServiceProvider();

            _notifyIcon = new TaskbarIcon
            {
                IconSource = new BitmapImage(new Uri("pack://application:,,,/Resources/app.ico")),
                ToolTipText = "Jct Weather Widget",
                ContextMenu = CreateContextMenu()
            };

            // Получаем логгер
            _logger = _serviceProvider.GetService<IJctLogger>();

            // Лог запуска приложения
            _logger?.LogInfo($"=== ЗАПУСК ПРИЛОЖЕНИЯ JctWeatherWidget v{GetAppVersion()} ===");
            _logger?.LogInfo($"Дата и время запуска: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            _logger?.LogInfo($"Операционная система: {Environment.OSVersion}");
            _logger?.LogInfo($"Версия .NET: {Environment.Version}");
            _logger?.LogInfo(new string('=', 60));

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        public T GetService<T>() => _serviceProvider.GetRequiredService<T>();

        private void ConfigureServices(ServiceCollection services)
        {
            // Регистрация логгера
            services.AddSingleton<IJctLogger>(provider =>
                new JctFileLogger("weather-widget.log", 5, 14));

            // Сервисы данных
            services.AddHttpClient<IWeatherService, JctWeatherApiService>(client =>
            {
                client.BaseAddress = new Uri("https://api.weatherapi.com/v1/");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("JctWeatherWidget/1.0");
            });

            services.AddSingleton<MainWindow>();
        }

        private static string GetAppVersion()
        {
            try
            {
                return System.Reflection.Assembly.GetExecutingAssembly()
                    .GetName().Version?.ToString() ?? "1.0.0.0";
            }
            catch
            {
                return "1.0.0.0";
            }
        }

        private ContextMenu CreateContextMenu()
        {
            var menu = new ContextMenu();

            var exitItem = new MenuItem
            {
                Header = "Выход"
            };
            exitItem.Click += (s, e) =>
            {
                _notifyIcon?.Dispose();
                Current.Shutdown();
            };

            menu.Items.Add(exitItem);
            return menu;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _logger?.LogInfo($"=== ЗАВЕРШЕНИЕ ПРИЛОЖЕНИЯ в {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===" +
                        $" (Код выхода: {e.ApplicationExitCode})");
            _logger?.LogInfo(new string('=', 60));

            _notifyIcon?.Dispose();
            base.OnExit(e);
        }
    }
}
