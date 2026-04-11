// Copyright (C) 2026 sarp_03
// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Snippy.Models.Cards;
using Snippy.Models.FileManagment.Snippets;

namespace Snippy.Views;

public partial class ManageSnippetsWindow : Window
{
    private static readonly string _appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private static readonly string _parentDirectory = Path.Combine(_appData, "Snippy");
    private static readonly string _configDirectory = Path.Combine(_parentDirectory, "Config");
    private static readonly string _snippetsDirectory = Path.Combine(_parentDirectory, "Snippets");
    
    
    private static readonly string _snippetsFilePath = Path.Combine(_snippetsDirectory, "SnippetList.json");
    private static readonly string _snippetFilesDirectory = Path.Combine(_snippetsDirectory, "SnippetFiles");
    
    public static ManageSnippetsWindow? Instance { get; private set; }
    
    public Dictionary<string, SnippetCard> SnippetCards = new();
    
    
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
            FontSize = 14
        };
        var authorTextBlock = new TextBlock
        {
            Text = "@" + snippet.Author,
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
            Content = "Edit",
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0,0,5,0),
            Tag = (snippet, index)
        };
        editSnippetButton.Bind(Button.BackgroundProperty, 
            editSnippetButton.GetResourceObservable("EditButtonColor"));
        editSnippetButton.Bind(Button.ForegroundProperty, editSnippetButton.GetResourceObservable("EditButtonTextColor"));
        editSnippetButton.Bind(Button.BorderBrushProperty, editSnippetButton.GetResourceObservable("EditButtonBorderColor"));
        
        editSnippetButton.Click += EditSnippetButton_OnClick;
        
        var deleteSnippetButton = new Button
        {
            Content = "Delete",
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Tag = (snippet, index)
        };
        deleteSnippetButton.Bind(Button.BackgroundProperty, 
            deleteSnippetButton.GetResourceObservable("DeleteButtonColor"));
        deleteSnippetButton.Bind(Button.ForegroundProperty, deleteSnippetButton.GetResourceObservable("DeleteButtonTextColor"));
        deleteSnippetButton.Bind(Button.BorderBrushProperty, deleteSnippetButton.GetResourceObservable("DeleteButtonBorderColor"));
        
        deleteSnippetButton.Click += DeleteSnippetButton_OnClick;
        
        Grid.SetColumn(childGrid, 0);
        Grid.SetColumn(editSnippetButton, 1);
        Grid.SetColumn(deleteSnippetButton, 2);
        
        parentGrid.Children.Add(childGrid);
        parentGrid.Children.Add(editSnippetButton);
        parentGrid.Children.Add(deleteSnippetButton);

        SnippetCards[snippet.ID] = new SnippetCard
        {
            TitleBlock = titleTextBlock,
            AuthorBlock = authorTextBlock,
            EditButton = editSnippetButton,
            DeleteButton = deleteSnippetButton
        };
        
        SnippetsList.Children.Add(parentGrid);

    }

    private async void EditSnippetButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var button  = sender as Button;
        var (snippet, index) = ((Snippet, int))button.Tag;
        
        SnippetEditorWindow editorWindow = new SnippetEditorWindow(false, true, null, snippet,  index);
        editorWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        await editorWindow.ShowDialog(this);
    }

    private async void DeleteSnippetButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var box = MessageBoxManager.GetMessageBoxStandard("Snipet Deletion",
            "Are you sure you want to delete this snippet?", ButtonEnum.YesNo);
        var result = await box.ShowAsync();

        if (result == ButtonResult.Yes)
        {
            var button  = sender as Button;
            var (snippet, index) = ((Snippet, int))button.Tag;

            string json = File.ReadAllText(_snippetsFilePath);
            SnippetManager snippetManager = JsonSerializer.Deserialize<SnippetManager>(json);
            
            string filePath = Path.Combine(_snippetFilesDirectory, snippet.ID + ".sh");
            
            snippetManager.Snippets.RemoveAll(s => s.ID == snippet.ID);
            File.Delete(filePath);
            
            string newJson = JsonSerializer.Serialize(snippetManager);
            File.WriteAllText(_snippetsFilePath, newJson);
            
            SnippetsList.Children.RemoveAt(index);
            MainWindow.Instance.SnippetList.Children.RemoveAt(index);
        }


    }

    private async void OpenEditor_OnClick(object? sender, RoutedEventArgs e)
    {
        SnippetEditorWindow editorWindow = new SnippetEditorWindow(true, false, null);
        editorWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        await editorWindow.ShowDialog(this);
    }

    private async void LoadFromURL_OnClick(object? sender, RoutedEventArgs e)
    {
        ImportURLWindow importWindow = new ImportURLWindow(); 
        await importWindow.ShowDialog(this);
    }
}