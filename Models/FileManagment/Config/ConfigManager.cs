// Copyright (C) 2026 sarp_03
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Generic;

namespace Snippy.Models.FileManagment.Config;

public class ConfigManager
{
    public List<ClientConnection> ClientConnections { get; set; } = new();
    public List<ClientPreferences> ClientPreferences { get; set; } = new();
}