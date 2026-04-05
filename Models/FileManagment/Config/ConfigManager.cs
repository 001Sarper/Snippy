using System.Collections.Generic;

namespace Snippy.Models.FileManagment.Config;

public class ConfigManager
{
    public List<ClientConnection> ClientConnections { get; set; } = new();
    public List<ClientPreferences> ClientPreferences { get; set; } = new();
}