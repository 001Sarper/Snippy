// Copyright (C) 2026 sarp_03
// SPDX-License-Identifier: GPL-3.0-or-later

using Avalonia.Controls;

namespace Snippy.Models.Cards;

public class SnippetCard
{
    public TextBlock TitleBlock { get; set; }
    public TextBlock AuthorBlock { get; set; }
    public TextBlock DescriptionBlock { get; set; }
    public Button ViewButton { get; set; }
    public Button ExecuteButton { get; set; }
    public Button EditButton { get; set; }
    public Button DeleteButton { get; set; }
}