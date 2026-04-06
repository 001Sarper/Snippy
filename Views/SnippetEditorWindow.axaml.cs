using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;
using AvaloniaEdit.Search;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Snippy.Models.FileManagment.Snippets;
using Snippet = Snippy.Models.FileManagment.Snippets.Snippet;

namespace Snippy.Views;

public partial class SnippetEditorWindow : Window
{
    private RegistryOptions _registryOptions;
    private TextMate.Installation _textMateInstallation;
    
    
    private static readonly string _appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private static readonly string _parentDirectory = Path.Combine(_appData, "Snippy");
    private static readonly string _configDirectory = Path.Combine(_parentDirectory, "Config");
    private static readonly string _snippetsDirectory = Path.Combine(_parentDirectory, "Snippets");
    
    private static readonly string _snippetsFilePath = Path.Combine(_snippetsDirectory, "SnippetList.json");
    private static readonly string _snippetFilesDirectory = Path.Combine(_snippetsDirectory, "SnippetFiles");
    
    private bool _isNewFile;
    private bool _isEditMode;
    private int _snippetIndex;
    
    
    public SnippetEditorWindow(bool isNewFile, bool isEditMode, Snippet? snippet = null, int snippetIndex = -1)
    {
        InitializeComponent();
        
        var editor = this.FindControl<TextEditor>("Editor");

        // 1. Initialize RegistryOptions with a default theme (e.g., DarkPlus)
        _registryOptions = new RegistryOptions(ThemeName.DarkPlus);

        // 2. Install TextMate support onto the editor
        _textMateInstallation = editor.InstallTextMate(_registryOptions);

        // 3. Set the grammar for a specific language (e.g., C#)
        var shellLanguage = _registryOptions.GetLanguageByExtension(".sh");
        string scopeName = _registryOptions.GetScopeByLanguageId(shellLanguage.Id);

        _textMateInstallation.SetGrammar(scopeName);
        
        SearchPanel.Install(editor);

        editor.Options.ShowTabs = true;
        editor.Options.EnableHyperlinks = true;
        editor.Options.EnableTextDragDrop = true;
        
        _isNewFile = isNewFile;
        _isEditMode = isEditMode;
        _snippetIndex = snippetIndex;
        
        if (!isNewFile)
        {
            SnippetNameTextBlock.Text = snippet.Name;
            FileAuthorTextBlock.Text = snippet.Author;
            FileDescriptionTextBlock.Text = snippet.Description;

            
            string filePath = Path.Combine(_snippetFilesDirectory, snippet.Path);
            string fileContent = File.ReadAllText(filePath);
            
            editor.Text = fileContent;
        
            editor.IsReadOnly = !isEditMode;

            SaveButton.Tag = snippet;
        }
        
    }

    private async void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var box = MessageBoxManager.GetMessageBoxStandard("Cancel", "Are you sure you want to cancel this operation?", ButtonEnum.YesNo);
        var result = await box.ShowAsync();

        if (result == ButtonResult.Yes) Close();
    }

    private async void SaveButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var editor = this.FindControl<TextEditor>("Editor");
        var button = sender as Button;
        var snippet = button.Tag as Snippet;

        SnippetManager snippetManager;

        if (File.Exists(_snippetsFilePath))
        {
            string existing = File.ReadAllText(_snippetsFilePath);
            snippetManager = JsonSerializer.Deserialize<SnippetManager>(existing);
            
        }
        else
        {
            snippetManager = new SnippetManager();
        }
        
        Snippet newSnippet = new Snippet
        {
            Name = SnippetNameTextBlock.Text,
            Author = FileAuthorTextBlock.Text,
            Description = FileDescriptionTextBlock.Text ?? string.Empty
        };
        
        
        if (_isNewFile && !string.IsNullOrWhiteSpace(editor.Text) && !string.IsNullOrWhiteSpace(SnippetNameTextBlock.Text) && !string.IsNullOrWhiteSpace(FileAuthorTextBlock.Text))
        {
            string filePath = Path.Combine(_snippetFilesDirectory, SnippetNameTextBlock.Text);
            var box = MessageBoxManager.GetMessageBoxStandard("Success", "New Snippet was added successfully.", ButtonEnum.Ok);
            
            File.WriteAllText(filePath, editor.Text, Encoding.UTF8);
            newSnippet.Path = SnippetNameTextBlock.Text + ".sh";
            snippetManager.Snippets.Add(newSnippet);
            string json = JsonSerializer.Serialize(snippetManager);
            File.WriteAllText(_snippetsFilePath, json);
            ManageSnippetsWindow.Instance.AddSnippet(newSnippet, _snippetIndex);
            MainWindow.Instance.AddSnippet(newSnippet);
            await box.ShowAsync();
            Close();



        } else if (!_isNewFile && _isEditMode && !string.IsNullOrWhiteSpace(editor.Text) && !string.IsNullOrWhiteSpace(SnippetNameTextBlock.Text) && !string.IsNullOrWhiteSpace(FileAuthorTextBlock.Text))
        {
            string filePath = Path.Combine(_snippetFilesDirectory, snippet.Path);
            var box = MessageBoxManager.GetMessageBoxStandard("Success", "Snippet was edited successfully");
            
            File.WriteAllText(filePath, editor.Text, Encoding.UTF8);
            newSnippet.Path = snippet.Path;
            snippetManager.Snippets[_snippetIndex]  = newSnippet;
            string json = JsonSerializer.Serialize(snippetManager);
            File.WriteAllText(_snippetsFilePath, json);
            
            MainWindow.Instance.FindControl<TextBlock>("TitleTextBlock-" + snippet.Name).Text = newSnippet.Name;
            MainWindow.Instance.FindControl<TextBlock>("AuthorTextBlock-" + snippet.Name).Text = newSnippet.Author;
            MainWindow.Instance.FindControl<TextBlock>("DescriptionTextBlock-" + snippet.Name).Text = newSnippet.Description;
            
            MainWindow.Instance.FindControl<Button>("ViewSnippetButton-" + snippet.Name).Tag = newSnippet;
            MainWindow.Instance.FindControl<Button>("ExecuteSnippetButton-" + snippet.Name).Tag = newSnippet;
            
            
            ManageSnippetsWindow.Instance.FindControl<TextBlock>("TitleTextBlock-" + snippet.Name).Text = newSnippet.Name;
            ManageSnippetsWindow.Instance.FindControl<TextBlock>("AuthorTextBlock-" + snippet.Name).Text = newSnippet.Author;
            
            ManageSnippetsWindow.Instance.FindControl<Button>("EditSnippetButton-" + snippet.Name).Tag = newSnippet;
            ManageSnippetsWindow.Instance.FindControl<Button>("DeleteSnippetButton-" + snippet.Name).Tag = newSnippet;

            await box.ShowAsync();
            Close();


        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Check Snippet Name, Author or Content!",
                ButtonEnum.Ok);
            await box.ShowAsync();
        }
    }
}