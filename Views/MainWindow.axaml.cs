using System;
using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Snippy.Models.FileManagment.Config;
using Snippy.Models.FileManagment.Snippets;
using Snippy.Views;

namespace Snippy;

public partial class MainWindow : Window
{
    private static readonly string _appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private static readonly string _parentDirectory = Path.Combine(_appData, "Snippy");
    private static readonly string _configDirectory = Path.Combine(_parentDirectory, "Config");
    private static readonly string _snippetsDirectory = Path.Combine(_parentDirectory, "Snippets");
    
    private static readonly string _connectionsFilePath = Path.Combine(_configDirectory, "ClientConnections.json");
    
    private static readonly string _snippetsFilePath = Path.Combine(_snippetsDirectory, "SnippetList.json");
    
    public static MainWindow? Instance { get; private set; }
    
    public MainWindow()
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
        var border = new Border
        {
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(16)
        };
        border.SetValue(Border.BackgroundProperty, new DynamicResourceExtension("SnippetColor"));

        var grid = new Grid
        {
            Height = 120,
            Width = 80
        };
        grid.RowDefinitions.Add(new RowDefinition(new GridLength(30)));
        grid.RowDefinitions.Add(new RowDefinition(new GridLength(20)));
        grid.RowDefinitions.Add(new RowDefinition(new GridLength(50)));
        grid.RowDefinitions.Add(new RowDefinition(new GridLength(20)));

        var titleTextBlock = new TextBlock
        {
            Text = snippet.Name,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Bold,
            FontSize = 14
        };

        var authorTextBlock = new TextBlock
        {
            Text = "@" + snippet.Author,
            HorizontalAlignment = HorizontalAlignment.Left,
            FontSize = 13
        };

        var descriptionTextBlock = new TextBlock
        {
            Text = snippet.Description,
            HorizontalAlignment = HorizontalAlignment.Left,
            FontSize = 11
        };

        var buttonsGrid = new Grid();
        buttonsGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(40)));
        buttonsGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(40)));

        var viewSnippetButton = new Button
        {
            Content = "View Snippet Content",
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Tag = snippet
        };
        viewSnippetButton.SetValue(Button.BackgroundProperty, "ViewButtonColor");
        
        var executeSnippetButton = new Button
        {
            Content = "Execute Snippet",
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Tag = snippet
        };
        executeSnippetButton.SetValue(Button.BackgroundProperty, "ExecuteButtonColor");
        
        Grid.SetColumn(viewSnippetButton, 0);
        Grid.SetColumn(executeSnippetButton, 1);
        
        buttonsGrid.Children.Add(viewSnippetButton);
        buttonsGrid.Children.Add(executeSnippetButton);
        
        Grid.SetRow(titleTextBlock, 0);
        Grid.SetRow(authorTextBlock, 1);
        Grid.SetRow(descriptionTextBlock, 2);
        Grid.SetRow(buttonsGrid, 3);
        
        grid.Children.Add(titleTextBlock);
        grid.Children.Add(authorTextBlock);
        grid.Children.Add(descriptionTextBlock);
        grid.Children.Add(buttonsGrid);
        
        border.Child = grid;
        
        



    }


    private async void ManageSnippetsMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        ManageSnippetsWindow manageSnippetsWindow = new ManageSnippetsWindow();
        await manageSnippetsWindow.ShowDialog(this);
    }
}