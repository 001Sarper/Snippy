// Copyright (C) 2026 sarp_03
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Snippy.Views;

public partial class AboutSnippyWindow : Window
{
    public AboutSnippyWindow()
    {
        InitializeComponent();
    }

    private void StarOnGithubButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            UseShellExecute = true,
            FileName = "https://github.com/001Sarper/Snippy"
        });
    }

    private void ReportIssueButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            UseShellExecute = true,
            FileName = "https://github.com/001Sarper/Snippy/issues/new"
        });
    }
}