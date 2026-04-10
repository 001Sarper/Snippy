// Copyright (C) 2026 sarp_03
// SPDX-License-Identifier: GPL-3.0-or-later

using Avalonia.Controls;
using WebViewControl;

namespace Snippy.Models.SSH;

public class SSH_Tab
{
    public WebView WebView { get; set; }
    public SSH_Service SshService { get; set; }
    public TerminalBridge Bridge { get; set; }
    public TabItem TabItem { get; set; }
}