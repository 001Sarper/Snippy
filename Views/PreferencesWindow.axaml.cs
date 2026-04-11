// Copyright (C) 2026 sarp_03
// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Snippy.Models.FileManagment.Config;
using WebViewControl;

namespace Snippy.Views;

public partial class PreferencesWindow : Window
{
    private static readonly string _appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private static readonly string _parentDirectory = Path.Combine(_appData, "Snippy");
    private static readonly string _configDirectory = Path.Combine(_parentDirectory, "Config");
    
    private static readonly string _preferencesFilePath = Path.Combine(_configDirectory, "ClientPreferences.json");
    
    public static string json = File.ReadAllText(_preferencesFilePath);
    public static ConfigManager configManager = JsonSerializer.Deserialize<ConfigManager>(json);
    
    public PreferencesWindow()
    {
        InitializeComponent();
        
        var savedTheme = configManager.ClientPreferences[0].Theme;
        var savedFontSize = configManager.ClientPreferences[0].FontSize;

        ThemeSelection.IsEditable = true;
        ThemeSelection.Text = savedTheme;
        ThemeSelection.IsEditable = false;
        FontSize.Value = savedFontSize;
    }

    private void Save_OnClick(object? sender, RoutedEventArgs e)
    {
        
        configManager.ClientPreferences[0].Theme = ThemeSelection.Text;
        configManager.ClientPreferences[0].FontSize = (int)FontSize.Value;

        File.WriteAllText(_preferencesFilePath, JsonSerializer.Serialize(configManager));

        App.Instance.SetTheme(ThemeSelection.Text);
        
        if (MainWindow.Instance.TerminalList.Any())
        {
            bool isDark = ThemeSelection.Text ==  "Dark";
            
            foreach (WebView webView in MainWindow.Instance.TerminalList)
            {
                webView.ExecuteScript($"term.options.fontSize = {(int)FontSize.Value};");
                webView.ExecuteScript($"setTheme({isDark.ToString().ToLower()})");
            }
            
        }
        
    }
}