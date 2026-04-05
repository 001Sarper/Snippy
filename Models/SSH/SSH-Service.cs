using System;
using System.IO;
using System.Text;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace Snippy.Models.SSH;


public class SSH_Service
{
    private SshClient _client;
    private ShellStream _shell;

    public void Connect(string host, int port, string user, string password, bool isPrivateKeyUsed, string privateKey, string passphrase, Action<string> onOutput, int cols = 80, int rows = 24)
    {
        Console.WriteLine($"SSH Connect with size: {cols}x{rows}");
        if (isPrivateKeyUsed)
        {
            var keyStream = new MemoryStream(Encoding.UTF8.GetBytes(privateKey));
            var keyFile = (string.IsNullOrEmpty(passphrase)) ? new PrivateKeyFile(keyStream) : new PrivateKeyFile(keyStream, passphrase);
            
            _client = new SshClient(host, port, user, keyFile);
        }
        else
        {
            _client = new SshClient(host, port, user, password);
        }
        
        _client.Connect();

        _shell = _client.CreateShellStream("xterm-256color", (uint)cols, (uint)rows, 0, 0, 1024);

        _shell.DataReceived += (sender, e) =>
        {
            var output = Encoding.UTF8.GetString(e.Data);
            onOutput(output);
        };
    }

    public void SendInput(string input)
    {
        _shell.Write(input);
    }

    public void Disconnect()
    {
        _shell?.Close();
        _client?.Disconnect();
        _client?.Dispose();
    }
    
    public void Resize(int cols, int rows)
    {
        Console.WriteLine($"Resize called: {cols}x{rows}");
        _shell?.ChangeWindowSize((uint)cols, (uint)rows, 0, 0);
    }
}