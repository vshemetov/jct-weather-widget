﻿using System.Windows;

namespace JctWeatherWidget;

public partial class App : Application
{
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
        mainWindow.Show();
    }
}
