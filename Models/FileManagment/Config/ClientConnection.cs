namespace Snippy.Models.FileManagment.Config;

public class ClientConnection
{
    public string Name { get; set; }
    public string Host  { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public bool PrivateKeyUsed { get; set; }
    public string PrivateKey { get; set; }
    public string Passphrase { get; set; }
}