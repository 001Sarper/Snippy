using System;
using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Microsoft.AspNetCore.DataProtection;
using Snippy.Models.FileManagment.Config;

namespace Snippy.Views;

public partial class ManageHostsWindow : Window
{
    private static readonly string _appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private static readonly string _parentDirectory = Path.Combine(_appData, "Snippy");
    private static readonly string _configDirectory = Path.Combine(_parentDirectory, "Config");
    
    private static readonly string _connectionsFilePath = Path.Combine(_configDirectory, "ClientConnections.json");
    
    
    public ManageHostsWindow()
    {
        InitializeComponent();
        
        if (File.Exists(_connectionsFilePath))
        {
            string json = File.ReadAllText(_connectionsFilePath);
            ConfigManager configManager = JsonSerializer.Deserialize<ConfigManager>(json);
            int index = 0;
            
            foreach (var connection in configManager.ClientConnections)
            {
                AddConnection(connection, index);
                index++;
            }
        }
        else
        {
            File.WriteAllText(_connectionsFilePath, "{}");
        }
    }
    
    private void AddConnection(ClientConnection connection, int index)
    {
        
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        
        var textblock = new TextBlock{Text = connection.Name, VerticalAlignment = VerticalAlignment.Center, Margin=new Thickness(0,0,5,0)};
        
        var editButton = new Button
        {
            Content = "Edit",
            Tag = (connection, index),
            HorizontalContentAlignment =  HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0,0,5,0),
            Width = 50,
            Height = 30
        };
        editButton.Bind(Button.BackgroundProperty, 
            editButton.GetResourceObservable("EditButtonColor"));
        editButton.Bind(Button.ForegroundProperty, editButton.GetResourceObservable("EditButtonTextColor"));
        editButton.Bind(Button.BorderBrushProperty, editButton.GetResourceObservable("EditButtonBorderColor"));
        
        var deleteButton = new Button
        {
            Content = "Delete",
            Tag = (connection, index),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Width = 75,
            Height = 30
        };
        deleteButton.Bind(Button.BackgroundProperty, 
            deleteButton.GetResourceObservable("DeleteButtonColor"));
        deleteButton.Bind(Button.ForegroundProperty, deleteButton.GetResourceObservable("DeleteButtonTextColor"));
        deleteButton.Bind(Button.BorderBrushProperty, deleteButton.GetResourceObservable("DeleteButtonBorderColor"));
        
        deleteButton.Click += DeleteConnection;
        editButton.Click += EditConnection;
        Grid.SetColumn(textblock, 0);
        Grid.SetColumn(editButton, 1);
        Grid.SetColumn(deleteButton, 2);
        
        grid.Children.Add(textblock);
        grid.Children.Add(editButton);
        grid.Children.Add(deleteButton);
        ParentPanel.Children.Add(grid);
        
        
        
        
    }
    
    private async void DeleteConnection(object? sender, RoutedEventArgs e)
    {
        var box = MessageBoxManager.GetMessageBoxStandard("Delete Connection",
            "Are you sure you want to delete this connection?", ButtonEnum.YesNo);
        var result = await box.ShowAsync();
        if (result == ButtonResult.Yes)
        {
            var button = sender as Button;
            var (connection, index) = ((ClientConnection, int))button.Tag;
        
            string json = File.ReadAllText(_connectionsFilePath);
            ConfigManager configManager = JsonSerializer.Deserialize<ConfigManager>(json);
        
            configManager.ClientConnections.RemoveAll(c => c.Name == connection.Name);
        
            string newJson = JsonSerializer.Serialize(configManager);
            File.WriteAllText(_connectionsFilePath, newJson);
            
            ParentPanel.Children.RemoveAt(index);

        } 
        
    }
    
    private void EditConnection(object? sender, RoutedEventArgs e)
    {
        
        var button = sender as Button;
        var (connection, index) = ((ClientConnection, int))button.Tag;
        
        ResetInputFields();
        
        BoxPanelButton.Content = "Save changes";
        
        BoxPanelButton.Tag = (index, 0);
        BoxPanelButton.Click += ManageConnection_OnClick;
        BoxPanelButton.Bind(Button.BackgroundProperty, 
            BoxPanelButton.GetResourceObservable("SaveButtonColor"));
        BoxPanelButton.Bind(Button.ForegroundProperty, BoxPanelButton.GetResourceObservable("SaveButtonTextColor"));
        BoxPanelButton.Bind(Button.BorderBrushProperty, BoxPanelButton.GetResourceObservable("SaveButtonBorderColor"));
        
        
        NameTextBox.Text = connection.Name;
        HostTextBox.Text = connection.Host;
        PortTextBox.Text = connection.Port.ToString();
        UserTextBox.Text = connection.Username;
        if (connection.PrivateKeyUsed)
        {
            AuthSelection.SelectedIndex = 1;
            PasswordAuth.IsVisible = false;
            PrivateKeyAuth.IsVisible = true;
            PassphraseTextBox.Text = App.Instance.Protector.Unprotect(connection.Passphrase);
        }
        else
        {
            AuthSelection.SelectedIndex = 0;
            PrivateKeyAuth.IsVisible = false;
            PasswordAuth.IsVisible = true;
            PasswordTextBox.Text = App.Instance.Protector.Unprotect(connection.Password);
        }
        
        BoxPanel.IsVisible = true;
        
        
        
    }
    
    private void AddConnection_OnClick(object? sender, RoutedEventArgs e)
    {
        ResetInputFields();
        AuthSelection.SelectedIndex = 0;
        
        BoxPanelButton.Content = "Add connection";
        BoxPanelButton.Tag = (-1, 1);
        BoxPanelButton.Click += ManageConnection_OnClick;
        BoxPanelButton.Bind(Button.BackgroundProperty, 
            BoxPanelButton.GetResourceObservable("AddButtonColor"));
        BoxPanelButton.Bind(Button.ForegroundProperty, BoxPanelButton.GetResourceObservable("AddButtonTextColor"));
        BoxPanelButton.Bind(Button.BorderBrushProperty, BoxPanelButton.GetResourceObservable("AddButtonBorderColor"));
        
        BoxPanel.IsVisible = true;
    }
    
    private async void ManageConnection_OnClick(object? sender, RoutedEventArgs e)
    {
        var button =  sender as Button;
        var (index, manageMode) = ((int, int))button.Tag;
        bool privKeyUsed = (AuthSelection.SelectedIndex == 1) ? true : false;
        
        string passphrase = (string.IsNullOrEmpty(PassphraseTextBox.Text)) ? "" : PassphraseTextBox.Text;
        
        ConfigManager configManager;
        
        if (File.Exists(_connectionsFilePath))
        {
            string existing = File.ReadAllText(_connectionsFilePath);
            configManager = JsonSerializer.Deserialize<ConfigManager>(existing);
        }
        else
        {
            configManager = new ConfigManager();
        }


        if (NameTextBox.Text != "" && HostTextBox.Text.Contains(".") && int.TryParse(PortTextBox.Text, out int port) &&
            UserTextBox.Text != "" && (privKeyUsed ? !string.IsNullOrEmpty(_privateKey) : PasswordTextBox.Text != ""))
        {
            ClientConnection newConnection = new ClientConnection()
            {
                Name = NameTextBox.Text,
                Host = HostTextBox.Text,
                Port = port,
                Username = UserTextBox.Text,
                Password = (!privKeyUsed) ? App.Instance.Protector.Protect(PasswordTextBox.Text) : "",
                PrivateKeyUsed = privKeyUsed,
                PrivateKey = (privKeyUsed) ? App.Instance.Protector.Protect(_privateKey) : "",
                Passphrase = (privKeyUsed && !string.IsNullOrEmpty(passphrase)) ? App.Instance.Protector.Protect(passphrase) : ""
            };
            
            if (manageMode == 0)
            {
                Console.WriteLine(NameTextBox.Text);
                configManager.ClientConnections[index] = newConnection;
                string json =  JsonSerializer.Serialize(configManager);
                File.WriteAllText(_connectionsFilePath, json);
                var box = MessageBoxManager.GetMessageBoxStandard("Connection changed successfully",
                    "Connection changed.", ButtonEnum.Ok);
                var grid = (Grid)ParentPanel.Children[index];
                var textblock = (TextBlock)grid.Children[0];
                var editButton =  (Button)grid.Children[1];
                editButton.Tag = (newConnection, index);
                textblock.Text = NameTextBox.Text;
                ResetInputFields();
                BoxPanel.IsVisible = false;
                await box.ShowAsync();
            }else if (manageMode == 1)
            {
                configManager.ClientConnections.Add(newConnection);
                string json =  JsonSerializer.Serialize(configManager);
                File.WriteAllText(_connectionsFilePath, json);
                var box = MessageBoxManager.GetMessageBoxStandard("Connection added successfully",
                    "Connection added.", ButtonEnum.Ok);
                AddConnection(newConnection, ParentPanel.Children.Count);
                ResetInputFields();
                BoxPanel.IsVisible = false;
                await box.ShowAsync();
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Invalid parameters",
                "Please enter valid connection parameters", ButtonEnum.Ok);
            await box.ShowAsync();
        }
        
    }
    
    private void ResetInputFields()
    {
        NameTextBox.Text = "";
        HostTextBox.Text = "";
        PortTextBox.Text = "";
        UserTextBox.Text = "";
        PasswordTextBox.Text = "";
        PassphraseTextBox.Text = "";
    }
    
    private void ShowPasswordButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var button = sender as Button;

        if (button.Tag == "PasswordAuth")
        {
            if (PasswordTextBox.RevealPassword == false)
            {
                PasswordTextBox.RevealPassword = true;
                button.Content = "Hide Password";
            }
            else
            {
                PasswordTextBox.RevealPassword = false;
                button.Content = "Show Password";
            }
        }
        else if (button.Tag == "PrivAuth")
        {
            if (PassphraseTextBox.RevealPassword == false)
            {
                PassphraseTextBox.RevealPassword = true;
                button.Content = "Hide Password";
            }
            else
            {
                PassphraseTextBox.RevealPassword = false;
                button.Content = "Show Password";
            }
        }
        
    }
    
    private void AuthSelection_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (BoxPanel is null) return;
        
        if (BoxPanel.IsVisible)
        {
            if (AuthSelection.SelectedIndex == 0)
            {
                PrivateKeyAuth.IsVisible = false;
                PasswordAuth.IsVisible = true;
            } else if (AuthSelection.SelectedIndex == 1)
            {
                PasswordAuth.IsVisible = false;
                PrivateKeyAuth.IsVisible = true;
            }
        }
    }
    
    private string _privateKey = string.Empty;

    private async void PickPrivateKey_OnClick(object? sender, RoutedEventArgs e)
    {
        var files = await TopLevel.GetTopLevel(this)!
            .StorageProvider
            .OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select private key file",
                AllowMultiple = false
            });

        if (files.Count > 0)
        {
            PrivateKeyPathBox.Text = files[0].Path.LocalPath;
            // Key einlesen:
            _privateKey = await File.ReadAllTextAsync(files[0].Path.LocalPath);
        }
    }
    
    
}