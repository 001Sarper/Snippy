using Avalonia;
using System;
using System.IO;
using System.Text.Json;
using Snippy.Models.FileManagment.Config;

namespace Snippy;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        EnsureConfigFiles();
        
        BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
    } 
    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    static void EnsureConfigFiles()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string parentDirectory = Path.Combine(appData, "Snippy");
        Directory.CreateDirectory(parentDirectory);
        
        string configDirectory = Path.Combine(parentDirectory, "Config");
        Directory.CreateDirectory(configDirectory);
        
        string connectionsFilePath = Path.Combine(configDirectory, "ClientConnections.json");
        string preferencesFilePath = Path.Combine(configDirectory, "ClientPreferences.json");
        
        string snippetsDirectory = Path.Combine(configDirectory, "Snippets");
        Directory.CreateDirectory(snippetsDirectory);
        
        string snippetsFilePath = Path.Combine(snippetsDirectory, "Snippets.json");
        


        if (!File.Exists(connectionsFilePath))
        {
            var defaultConnections = new
            {
                Connections = Array.Empty<object>()
            };
            File.WriteAllText(connectionsFilePath, JsonSerializer.Serialize(defaultConnections, new JsonSerializerOptions { WriteIndented = true }));
        }
        
        if (!File.Exists(preferencesFilePath))
        {
            var defaultPrefs = new 
            { 
                ClientPreferences = new[] { new { Theme = "Dark", FontSize = 12 } } 
            };
            File.WriteAllText(preferencesFilePath, JsonSerializer.Serialize(defaultPrefs, new JsonSerializerOptions { WriteIndented = true }));
        }

        if (!File.Exists(snippetsFilePath))
        {
            var defaultSnippets = new
            {
                Snippets = Array.Empty<object>()
            };
            File.WriteAllText(snippetsFilePath, JsonSerializer.Serialize(defaultSnippets, new JsonSerializerOptions { WriteIndented = true }));
        }
        
        
    }
}
