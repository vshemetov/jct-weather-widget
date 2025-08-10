using JctWeatherWidget.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace JctWeatherWidget
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();

            // Добавляем HttpClient
            services.AddHttpClient<JctWeatherService>(client =>
            {
                client.BaseAddress = new Uri("https://api.weatherapi.com/v1/");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("JctWeatherWidget/1.0");
            });

            // Регистрируем MainWindow
            services.AddSingleton<MainWindow>();

            // Регистрируем WeatherService с HttpClient
            services.AddSingleton<JctWeatherService>();

            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        public T GetService<T>() => _serviceProvider.GetRequiredService<T>();
    }
}
