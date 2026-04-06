using System;
using System.Diagnostics;
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

    public void AddSnippet(Snippet snippet)
    {
        var border = new Border
        {
            CornerRadius = new CornerRadius(12),
            Width = 160,
            Height = 220,
            Padding = new Thickness(16),
            BorderThickness = new Thickness(0.5)
        };
        border[!Border.BackgroundProperty] = new DynamicResourceExtension("SnippetColor");
        border[!Border.BorderBrushProperty] = new DynamicResourceExtension("SnippetBorderColor");

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));     // Row 0 - Title + Separator
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));     // Row 1 - Author
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));     // Row 2 - Description
        grid.RowDefinitions.Add(new RowDefinition(new GridLength(1, GridUnitType.Star))); // Row 3 - Spacer
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));     // Row 4 - Buttons

        var titleTextBlock = new TextBlock
        {
            Text = snippet.Name,
            Name = "TitleTextBlock-" + snippet.Name,
            HorizontalAlignment = HorizontalAlignment.Center,
            FontWeight = FontWeight.Medium,
            FontSize = 14,
            Margin = new Thickness(0, 0, 0, 8)
        };

        var titleSeparator = new Border
        {
            BorderThickness = new Thickness(0, 0, 0, 0.5),
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 0, 8)
        };
        titleSeparator[!Border.BorderBrushProperty] = new DynamicResourceExtension("SnippetBorderColor");

        var authorTextBlock = new TextBlock
        {
            Text = "@" + snippet.Author,
            Name = "AuthorTextBlock-" + snippet.Name,
            HorizontalAlignment = HorizontalAlignment.Left,
            FontSize = 12,
            Margin = new Thickness(0, 0, 0, 6)
        };
        authorTextBlock[!TextBlock.ForegroundProperty] = new DynamicResourceExtension("AuthorTextColor");

        var descriptionTextBlock = new TextBlock
        {
            Text = snippet.Description,
            Name = "DescriptionTextBlock-" + snippet.Name,
            HorizontalAlignment = HorizontalAlignment.Left,
            FontSize = 11,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 17
        };
        descriptionTextBlock[!TextBlock.ForegroundProperty] = new DynamicResourceExtension("SecondaryTextColor");

        var buttonsGrid = new Grid();
        buttonsGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
        buttonsGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(6)));
        buttonsGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));

        var viewSnippetButton = new Button
        {
            Content = "View",
            Name = "ViewSnippetButton-" + snippet.Name,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            FontSize = 11,
            Padding = new Thickness(0, 5, 0, 5),
            Tag = snippet
        };
        viewSnippetButton.Bind(Button.BackgroundProperty, viewSnippetButton.GetResourceObservable("ViewButtonColor"));
        viewSnippetButton.Bind(Button.ForegroundProperty, viewSnippetButton.GetResourceObservable("ViewButtonTextColor"));
        viewSnippetButton.Bind(Button.BorderBrushProperty, viewSnippetButton.GetResourceObservable("ViewButtonBorderColor"));

        viewSnippetButton.Click += ViewSnippetButton_OnClick;

        var executeSnippetButton = new Button
        {
            Content = "Execute",
            Name = "ExecuteSnippetButton-" + snippet.Name,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            FontSize = 11,
            Padding = new Thickness(0, 5, 0, 5),
            Tag = snippet
        };
        executeSnippetButton.Bind(Button.BackgroundProperty, executeSnippetButton.GetResourceObservable("ExecuteButtonColor"));
        executeSnippetButton.Bind(Button.ForegroundProperty, executeSnippetButton.GetResourceObservable("ExecuteButtonTextColor"));
        executeSnippetButton.Bind(Button.BorderBrushProperty, executeSnippetButton.GetResourceObservable("ExecuteButtonBorderColor"));

        Grid.SetColumn(viewSnippetButton, 0);
        Grid.SetColumn(executeSnippetButton, 2);

        buttonsGrid.Children.Add(viewSnippetButton);
        buttonsGrid.Children.Add(executeSnippetButton);

        Grid.SetRow(titleTextBlock, 0);
        Grid.SetRow(titleSeparator, 0);
        Grid.SetRow(authorTextBlock, 1);
        Grid.SetRow(descriptionTextBlock, 2);
        Grid.SetRow(buttonsGrid, 4);

        grid.Children.Add(titleTextBlock);
        grid.Children.Add(titleSeparator);
        grid.Children.Add(authorTextBlock);
        grid.Children.Add(descriptionTextBlock);
        grid.Children.Add(buttonsGrid);

        border.Child = grid;
        SnippetList.Children.Add(border);
    }

    private async void ViewSnippetButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var snippet = button.Tag as Snippet;
        SnippetEditorWindow snippetEditorWindow = new SnippetEditorWindow(false, false, snippet);
        snippetEditorWindow.SnippetNameTextBlock.IsReadOnly = true;
        snippetEditorWindow.FileAuthorTextBlock.IsReadOnly = true;
        snippetEditorWindow.FileDescriptionTextBlock.IsReadOnly = true;
        snippetEditorWindow.BottomSeparator.IsVisible = false;
        snippetEditorWindow.ButtonsGrid.IsVisible = false;
        await snippetEditorWindow.ShowDialog(this);
    }


    private async void ManageSnippetsMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        ManageSnippetsWindow manageSnippetsWindow = new ManageSnippetsWindow();
        await manageSnippetsWindow.ShowDialog(this);
    }

    private async void PreferencesMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        PreferencesWindow preferencesWindow = new PreferencesWindow();
        await preferencesWindow.ShowDialog(this);
    }

    private void ExitMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        Environment.Exit(0);
    }

    private void OpenGithubMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            UseShellExecute = true,
            FileName = "https://github.com/001Sarper/Snippy"
        });
    }

    private async void ManageHostsMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        ManageHostsWindow manageHostsWindow = new ManageHostsWindow();
        await manageHostsWindow.ShowDialog(this);
    }
}