using System;
using System.IO;
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
    
    
    public SnippetEditorWindow(bool isNewFile, bool isEditMode, Snippet? snippet = null)
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
        
        if (!isNewFile)
        {
            SnippetNameTextBlock.Text = snippet.Name;
            FileAuthorTextBlock.Text = snippet.Author;
            FileDescriptionTextBlock.Text = snippet.Description;

            
            string filePath = Path.Combine(_snippetFilesDirectory, snippet.Path);
            string fileContent = File.ReadAllText(filePath);
            
            editor.Text = fileContent;
        
            editor.IsReadOnly = !isEditMode;
        }
        
    }

    private async void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var box = MessageBoxManager.GetMessageBoxStandard("Cancel", "Are you sure you want to cancel this operation?", ButtonEnum.YesNo);
        var result = await box.ShowAsync();

        if (result == ButtonResult.Yes) Close();
    }
}