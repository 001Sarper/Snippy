using System;
using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
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
            int index = 0;
            
            foreach (var snippet in snippets.Snippets)
            {
                AddSnippet(snippet, index);
                index++;
            }
        }
    }

    public void AddSnippet(Snippet snippet, int index)
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
            Name = "TitleTextBlock-" + snippet.Name,
            FontSize = 14
        };
        var authorTextBlock = new TextBlock
        {
            Text = "@" + snippet.Author,
            Name =  "AuthorTextBlock-" + snippet.Name,
            Margin = new Thickness(0,5,0,0),
            FontSize = 12
        };
        authorTextBlock[!TextBlock.ForegroundProperty] = new DynamicResourceExtension("AuthorTextColor");
        
        Grid.SetRow(titleTextBlock, 0);
        Grid.SetRow(authorTextBlock, 1);
        
        childGrid.Children.Add(titleTextBlock);
        childGrid.Children.Add(authorTextBlock);

        var editSnippetButton = new Button
        {
            Content = "Edit Snippet",
            Name =  "EditSnippetButton-" + snippet.Name,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Tag = (snippet, index)
        };
        editSnippetButton.Bind(Button.BackgroundProperty, 
            editSnippetButton.GetResourceObservable("ViewButtonColor"));
        editSnippetButton.Click += EditSnippetButton_OnClick;
        
        var deleteSnippetButton = new Button
        {
            Content = "Delete Snippet",
            Name =  "DeleteSnippetButton-" + snippet.Name,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Tag = (snippet, index)
        };
        deleteSnippetButton.Bind(Button.BackgroundProperty, 
            deleteSnippetButton.GetResourceObservable("DeleteButtonColor"));
        editSnippetButton.Click += DeleteSnippetButton_OnClick;
        
        Grid.SetColumn(childGrid, 0);
        Grid.SetColumn(editSnippetButton, 1);
        Grid.SetColumn(deleteSnippetButton, 2);
        
        parentGrid.Children.Add(childGrid);
        parentGrid.Children.Add(editSnippetButton);
        parentGrid.Children.Add(deleteSnippetButton);
        
        SnippetsList.Children.Add(parentGrid);

    }

    private async void EditSnippetButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var button  = sender as Button;
        var (snippet, index) = ((Snippet, int))button.Tag;
        
        SnippetEditorWindow editorWindow = new SnippetEditorWindow(false, true, snippet,  index);
        await editorWindow.ShowDialog(this);
    }

    private async void DeleteSnippetButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var button  = sender as Button;
        var (snippet, index) = ((Snippet, int))button.Tag;
        
        
    }

    private async void OpenEditor_OnClick(object? sender, RoutedEventArgs e)
    {
        SnippetEditorWindow editorWindow = new SnippetEditorWindow(true, false);
        await editorWindow.ShowDialog(this);
    }
}