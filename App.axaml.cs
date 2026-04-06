using System;
using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Org.BouncyCastle.Math.EC;
using Snippy.Models.FileManagment.Config;

namespace Snippy;

public partial class App : Application
{
    public static App Instance { get; private set; }
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        Instance = this;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
        
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string parentDirectory = Path.Combine(appData, "Snippy");
        string configDirectory = Path.Combine(parentDirectory, "Config");
        string preferencesFilePath = Path.Combine(configDirectory, "ClientPreferences.json");

        if (File.Exists(preferencesFilePath))
        {
            string json = File.ReadAllText(preferencesFilePath);
            ConfigManager configManager = JsonSerializer.Deserialize<ConfigManager>(json);
            var preference = configManager.ClientPreferences[0];
            SetTheme(preference.Theme);
        }
        else
        {
            RequestedThemeVariant = ThemeVariant.Dark; // Standard falls keine Config da
        }
    }
    
    public void SetTheme(string theme)
    {
        RequestedThemeVariant = theme == "Dark" ? ThemeVariant.Dark : ThemeVariant.Light;
    }
}