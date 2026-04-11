// Copyright (C) 2026 sarp_03
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Text.Json.Serialization;

namespace Snippy.Models.FileManagment.Snippets;

public class SnippyJson
{
    public string Name { get; set; } = "";
    public string Author { get; set; } = "";
    public string Description { get; set; } = "";
    
    [JsonPropertyName("script_url")]
    public string ScriptUrl { get; set; } = "";
}