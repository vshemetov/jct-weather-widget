using Hardcodet.Wpf.TaskbarNotification;
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

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();

            // Добавляем HttpClient
            services.AddHttpClient<IWeatherService, JctWeatherApiService>(client =>
            {
                client.BaseAddress = new Uri("https://api.weatherapi.com/v1/");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("JctWeatherWidget/1.0");
            });

            // Регистрируем MainWindow
            services.AddSingleton<MainWindow>();

            // Регистрируем WeatherService с HttpClient
            services.AddSingleton<JctWeatherApiService>();
            _serviceProvider = services.BuildServiceProvider();

            _notifyIcon = new TaskbarIcon
            {
                IconSource = new BitmapImage(new Uri("pack://application:,,,/Resources/app.ico")),
                ToolTipText = "Jct Weather Widget",
                ContextMenu = CreateContextMenu()
            };

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        public T GetService<T>() => _serviceProvider.GetRequiredService<T>();

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
            _notifyIcon?.Dispose();
            base.OnExit(e);
        }
    }
}
