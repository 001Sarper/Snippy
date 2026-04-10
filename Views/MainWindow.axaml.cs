// Copyright (C) 2026 sarp_03
// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using MsBox.Avalonia;
using Snippy.Models.FileManagment.Config;
using Snippy.Models.FileManagment.Snippets;
using Snippy.Models.SSH;
using Snippy.Views;
using Microsoft.AspNetCore.DataProtection;
using Snippy.Models.Cards;
using WebViewControl;

namespace Snippy;

public partial class MainWindow : Window
{
    private static readonly string _appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private static readonly string _parentDirectory = Path.Combine(_appData, "Snippy");
    private static readonly string _configDirectory = Path.Combine(_parentDirectory, "Config");
    private static readonly string _snippetsDirectory = Path.Combine(_parentDirectory, "Snippets");
    
    private static readonly string _connectionsFilePath = Path.Combine(_configDirectory, "ClientConnections.json");
    private static readonly string _preferencesFilePath = Path.Combine(_configDirectory, "ClientPreferences.json");
    
    private static readonly string _snippetsFilePath = Path.Combine(_snippetsDirectory, "SnippetList.json");
    private static readonly string _snippetFilesDirectory = Path.Combine(_snippetsDirectory, "SnippetFiles");
    
    private List<ClientConnection> _checkedServers = new List<ClientConnection>();
    
    public List<WebView> TerminalList = new List<WebView>();

    public Dictionary<string, SnippetCard> SnippetCards = new();
    
    public static MainWindow? Instance { get; private set; }
    
    public MainWindow()
    {
        Instance = this;
        
        InitializeComponent();

        if (File.Exists(_snippetsFilePath))
        {
            string snippetsJson = File.ReadAllText(_snippetsFilePath);
            SnippetManager? snippetManager = JsonSerializer.Deserialize<SnippetManager>(snippetsJson);

            foreach (var snippet in snippetManager.Snippets)
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
            Width = 180,
            Height = 220,
            Padding = new Thickness(16),
            BorderThickness = new Thickness(1)
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
            HorizontalAlignment = HorizontalAlignment.Center,
            FontWeight = FontWeight.Medium,
            FontSize = 14,
            TextWrapping = TextWrapping.Wrap,
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
            HorizontalAlignment = HorizontalAlignment.Left,
            FontSize = 12,
            Margin = new Thickness(0, 0, 0, 6)
        };
        authorTextBlock[!TextBlock.ForegroundProperty] = new DynamicResourceExtension("AuthorTextColor");

        var descriptionTextBlock = new TextBlock
        {
            Text = snippet.Description,
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
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            FontSize = 11,
            Padding = new Thickness(0, 5, 0, 5),
            Tag = snippet
        };
        executeSnippetButton.Bind(Button.BackgroundProperty, executeSnippetButton.GetResourceObservable("ExecuteButtonColor"));
        executeSnippetButton.Bind(Button.ForegroundProperty, executeSnippetButton.GetResourceObservable("ExecuteButtonTextColor"));
        executeSnippetButton.Bind(Button.BorderBrushProperty, executeSnippetButton.GetResourceObservable("ExecuteButtonBorderColor"));

        executeSnippetButton.Click += ExecuteSnippetButton_OnClick;
        
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

        SnippetCards[snippet.ID] = new SnippetCard
        {
            TitleBlock = titleTextBlock,
            AuthorBlock = authorTextBlock,
            DescriptionBlock = descriptionTextBlock,
            ViewButton = viewSnippetButton,
            ExecuteButton = executeSnippetButton
        };
        
        
        SnippetList.Children.Add(border);
        
        
        
    }

    private void ExecuteSnippetButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var snippet = button.Tag as Snippet;

        ServerSelectionSection.Children.Clear();
        
        if (File.Exists(_connectionsFilePath))
        {
            string connectionsJson = File.ReadAllText(_connectionsFilePath);
            ConfigManager? configManager = JsonSerializer.Deserialize<ConfigManager>(connectionsJson);
        
            foreach (var connection in configManager.ClientConnections)
            {
                AddConnection(connection);
            }
        }
        var column = ConnectionGrid.ColumnDefinitions[2];
        
        _checkedServers.Clear();
        ConnectionList.Tag = column;
        ServerSelectionConfirmButton.Tag = snippet;
        ConnectionListSplitter.IsVisible = true;
        ConnectionList.IsVisible = true;
        column.MinWidth = 220;
        column.Width = GridLength.Auto;
        
    }

    private void AddConnection(ClientConnection connection)
    {
        var border = new Border
        {
            CornerRadius = new CornerRadius(12),
            Margin = new Thickness(5),
            HorizontalAlignment =  HorizontalAlignment.Stretch,
            Height = 40,
            Padding = new Thickness(2),
            BorderThickness = new Thickness(1),
            VerticalAlignment = VerticalAlignment.Top,
        };
        border[!BackgroundProperty] = new DynamicResourceExtension("ServerListSectionBackground");
        border[!BorderBrushProperty] = new DynamicResourceExtension("ServerListSectionBorder");
        
        
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        
        
        var textblock = new TextBlock{Text = connection.Name, VerticalAlignment = VerticalAlignment.Center, FontSize = 14, Margin=new Thickness(5,0,5,0)};
        var checkbox = new CheckBox
        {
            Tag = connection
        };
        checkbox.IsCheckedChanged += CheckboxOnIsCheckedChanged;
        
        
        Grid.SetColumn(textblock, 0);
        Grid.SetColumn(checkbox, 2);
        
        grid.Children.Add(textblock);
        grid.Children.Add(checkbox);
        border.Child = grid;
        ServerSelectionSection.Children.Add(border);
        
        
        
    }

    private void CheckboxOnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        var checkbox = sender as CheckBox;
        var connection = checkbox.Tag as ClientConnection;
        bool isChecked = checkbox.IsChecked ?? false;
        if (isChecked) _checkedServers.Add(connection); else _checkedServers.Remove(connection);
    }


    private async void ViewSnippetButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var snippet = button.Tag as Snippet;
        SnippetEditorWindow snippetEditorWindow = new SnippetEditorWindow(false, false, snippet);
        snippetEditorWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        snippetEditorWindow.SnippetNameTextBlock.IsReadOnly = true;
        snippetEditorWindow.FileAuthorTextBlock.IsReadOnly = true;
        snippetEditorWindow.FileDescriptionTextBlock.IsReadOnly = true;
        snippetEditorWindow.BottomSeparator.IsVisible = false;
        snippetEditorWindow.ButtonsGrid.IsVisible = false;
        await snippetEditorWindow.ShowDialog(this);
    }


    private async void ManageSnippetsMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ConnectionList.IsVisible && ConnectionListSplitter.IsVisible)
        {
            ConnectionList.IsVisible = false;
            ConnectionListSplitter.IsVisible = false;
        }
        
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
        if (ConnectionList.IsVisible && ConnectionListSplitter.IsVisible)
        {
            ConnectionList.IsVisible = false;
            ConnectionListSplitter.IsVisible = false;
        }
        
        ManageHostsWindow manageHostsWindow = new ManageHostsWindow();
        await manageHostsWindow.ShowDialog(this);
    }

    private async void ConfirmServersButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var button =  sender as Button;
        var snippet = button.Tag as Snippet;
        
        if (_checkedServers.Count > 0)
        {
            string filePath = Path.Combine(_snippetFilesDirectory, snippet.Path);
            string snippetContent = File.ReadAllText(filePath);
            
            foreach (var server in _checkedServers)
            {
                string json = File.ReadAllText(_preferencesFilePath);
                ConfigManager configManager = JsonSerializer.Deserialize<ConfigManager>(json);

                SSH_Tab tab = new SSH_Tab()
                {
                    WebView = new WebView(),
                    SshService = new SSH_Service(),
                    Bridge = new TerminalBridge()
                };
                
                TerminalList.Add(tab.WebView);
                
                TabItem tabItem = new TabItem
                {
                    FontSize = 12,
                    IsSelected =  true,
                    Tag = tab
                };
                
                var headerPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
                var headerText = new TextBlock { Text = snippet.Name + " @ " + server.Name, VerticalAlignment = VerticalAlignment.Center };
                var closeButton = new Button
                {
                    Content = "✕",
                    FontSize = 10,
                    Padding = new Thickness(4, 1),
                    VerticalAlignment = VerticalAlignment.Center
                };

                closeButton.Click += (s, args) =>
                {
                    TabController.Items.Remove(tabItem);
                    // Cleanup
                    tab.SshService?.Disconnect();
                    tab.WebView?.Dispose();
                };

                headerPanel.Children.Add(headerText);
                headerPanel.Children.Add(closeButton);
                tabItem.Header = headerPanel;
        
                tabItem.GotFocus += SelectInput_OnClick;
                tabItem.Content = tab.WebView;
                
                TabController.Items.Add(tabItem);
                
                var htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "xTerm.js", "terminal.html");
                var theme = configManager.ClientPreferences[0].Theme.ToLower();
                var fontSize = configManager.ClientPreferences[0].FontSize;
                
                tab.Bridge = new TerminalBridge();
                tab.Bridge.OnResize = (cols, rows) => tab.SshService.Resize(cols, rows);
                
                tab.WebView.Loaded += (s, e) =>
                {
                    tab.WebView.RegisterJavascriptObject("terminalBridge", tab.Bridge);
                    _ = Task.Run(() => ConnectSSH(server, tab, tabItem, snippetContent));
                };

                tab.WebView.Address = $"file://{htmlPath}?fontSize={fontSize}&theme={theme}";
                    
                
                
            }
            
            ConnectionListSplitter.IsVisible = false;
            ConnectionList.IsVisible = false;
            var column = ConnectionGrid.ColumnDefinitions[2];
            column.MinWidth = 0;
            column.Width = new GridLength(0);
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "There were no servers selected!");
            await box.ShowAsync();
        }
    }
    
    private async Task ConnectSSH(ClientConnection connection, SSH_Tab connectionTab, TabItem tabItem, string snippetContent)
    {
        connectionTab.SshService.Disconnect();
        bool privKeyUsed = connection.PrivateKeyUsed;

        try
        {
            connectionTab.SshService.Connect(
                connection.Host,
                connection.Port,
                connection.Username,
                !privKeyUsed ? App.Instance.Protector.Unprotect(connection.Password) : "",
                privKeyUsed,
                privKeyUsed && !string.IsNullOrEmpty(connection.PrivateKey)
                    ? App.Instance.Protector.Unprotect(connection.PrivateKey)
                    : "",
                privKeyUsed && !string.IsNullOrEmpty(connection.Passphrase)
                    ? App.Instance.Protector.Unprotect(connection.Passphrase)
                    : "", snippetContent,
                (output) =>
                {
                    var safe = output.Replace("`", "\\`").Replace("\\", "\\\\");
                    connectionTab.WebView.ExecuteScript($"term.write(`{safe}\r\n`)");
                    
                }, connectionTab.Bridge.Cols, connectionTab.Bridge.Rows);
        }
        catch (Exception e)
        {
            TabController.Items.Remove(tabItem);
            // Cleanup
            connectionTab.SshService?.Disconnect();
            connectionTab.WebView?.Dispose();
            
            var box = MessageBoxManager.GetMessageBoxStandard("Couldn't connect to SSH", "Connection to the SSH Server could not be estabilished. Error: \n" + e);
            await box.ShowAsync();
        }
    }
    
    private void SelectInput_OnClick(object? sender, RoutedEventArgs e)
    {
        var button = sender as TabItem;
        var currentTab =  button.Tag as SSH_Tab;
        
        currentTab.Bridge.OnResize = (cols, rows) => currentTab.SshService.Resize(cols, rows);
        currentTab.WebView.RegisterJavascriptObject("terminalBridge", currentTab.Bridge);
        currentTab.WebView.Focus();
        
    }

    private void CloseServerSelectionSection_OnClick(object? sender, RoutedEventArgs e)
    {
        ConnectionList.IsVisible = false;
        ConnectionListSplitter.IsVisible = false;
        var column = ConnectionGrid.ColumnDefinitions[2];
        column.MinWidth = 0;
        column.Width = new GridLength(0);

    }

    private async void AboutSnippyMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        AboutSnippyWindow aboutSnippyWindow = new  AboutSnippyWindow();
        await aboutSnippyWindow.ShowDialog(this);
    }
}