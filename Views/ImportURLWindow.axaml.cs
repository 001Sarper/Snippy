// Copyright (C) 2026 sarp_03
// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Net.Http;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MsBox.Avalonia;
using Org.BouncyCastle.Security;
using Snippy.Models.FileManagment.Snippets;

namespace Snippy.Views;

public partial class ImportURLWindow : Window
{
    private readonly HttpClient _httpClient = new();
    
    public ImportURLWindow()
    {
        InitializeComponent();
    }

    private void Cancel_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void Import_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(UrlTextBox.Text))
        {
            
            try
            {
                var jsonURL = NormalizeToRawUrl(UrlTextBox.Text);
                var jsonContent = await _httpClient.GetStringAsync(jsonURL);

                var snippyJson = JsonSerializer.Deserialize<SnippyJson>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? throw new Exception("Failed to parse snippet json. Invalid JSON format");

                var scriptUrl = NormalizeToRawUrl(snippyJson.ScriptUrl);
                var scriptContent = await _httpClient.GetStringAsync(scriptUrl);
                
                Snippet importedSnippet = new Snippet
                {
                    Name = snippyJson.Name,
                    Author = snippyJson.Author,
                    Description = snippyJson.Description
                };
                
                SnippetEditorWindow snippetEditorWindow = new SnippetEditorWindow(false, false, scriptContent, importedSnippet);
                snippetEditorWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                snippetEditorWindow.Show();
                Close();
                
            }
            catch (HttpRequestException ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "The given URL couldn't be reached. Exception: " + ex.Message);
                await box.ShowAsync();
            }
            
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "There was no URL given");
            await box.ShowAsync();
        }
        
    }
    
    private static string NormalizeToRawUrl(string url)
    {
        // https://github.com/001Sarper/Snippy-Hub/blob/main/LICENSE
        // → https://raw.githubusercontent.com/001Sarper/Snippy-Hub/refs/heads/main/LICENSE
        if (url.Contains("github.com") && url.Contains("/blob/"))
        {
            return url
                .Replace("github.com", "raw.githubusercontent.com")
                .Replace("/blob/", "/refs/heads/");
        }

        return url; 
    }
}