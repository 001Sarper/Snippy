using System;
using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Snippy.Models.FileManagment.Snippets;

namespace Snippy.Views;

public partial class ManageSnippetsWindow : Window
{
    private static readonly string _appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private static readonly string _parentDirectory = Path.Combine(_appData, "Snippy");
    private static readonly string _configDirectory = Path.Combine(_parentDirectory, "Config");
    private static readonly string _snippetsDirectory = Path.Combine(_parentDirectory, "Snippets");
    
    private static readonly string _connectionsFilePath = Path.Combine(_configDirectory, "ClientConnections.json");
    
    private static readonly string _snippetsFilePath = Path.Combine(_snippetsDirectory, "SnippetList.json");
    
    public static ManageSnippetsWindow? Instance { get; private set; }
    
    
    public ManageSnippetsWindow()
    {
        Instance = this;
        
        InitializeComponent();
        
        if (File.Exists(_snippetsFilePath))
        {
            string json = File.ReadAllText(_snippetsFilePath);
            SnippetManager? snippets = JsonSerializer.Deserialize<SnippetManager>(json);

            foreach (var snippet in snippets.Snippets)
            {
                AddSnippet(snippet);
            }
        }
    }

    private void AddSnippet(Snippet snippet)
    {
        var parentGrid = new Grid();
        
        parentGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        parentGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        parentGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        
        var childGrid = new Grid();
        childGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        childGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        var titleTextBlock = new TextBlock
        {
            Text = snippet.Name,
            FontSize = 14
        };
        var authorTextBlock = new TextBlock
        {
            Text = "@" + snippet.Author,
            FontSize = 12
        };
        
        Grid.SetRow(titleTextBlock, 0);
        Grid.SetRow(authorTextBlock, 1);
        
        childGrid.Children.Add(titleTextBlock);
        childGrid.Children.Add(authorTextBlock);

        var editSnippetButton = new Button
        {
            Content = "Edit Snippet",
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Tag = snippet
        };
        editSnippetButton.SetValue(Button.BackgroundProperty, "ViewButtonColor");
        
        var executeSnippetButton = new Button
        {
            Content = "Execute Snippet",
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Tag = snippet
        };
        executeSnippetButton.SetValue(Button.BackgroundProperty, "ExecuteButtonColor");
        
        Grid.SetColumn(childGrid, 0);
        Grid.SetColumn(editSnippetButton, 1);
        Grid.SetColumn(executeSnippetButton, 2);
        
        parentGrid.Children.Add(childGrid);
        parentGrid.Children.Add(editSnippetButton);
        parentGrid.Children.Add(executeSnippetButton);
        
        SnippetsList.Children.Add(parentGrid);

    }

    private async void OpenEditor_OnClick(object? sender, RoutedEventArgs e)
    {
        SnippetEditorWindow editorWindow = new SnippetEditorWindow(true, false);
        await editorWindow.ShowDialog(this);
    }
}