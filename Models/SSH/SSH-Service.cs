// Copyright (C) 2026 sarp_03
// SPDX-License-Identifier: GPL-3.0-or-later

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

    public void Connect(string host, int port, string user, string password, bool isPrivateKeyUsed, string privateKey, string passphrase, string snippetContent, Action<string> onOutput, int cols = 80, int rows = 24)
    {
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
        
        var cmd = _client.CreateCommand(snippetContent);
        var result = cmd.BeginExecute();
        var stream = cmd.OutputStream;
        var encoding = Encoding.UTF8;

        using (var reader = new StreamReader(stream, encoding, true, 1024, true))
        {
            while (!result.IsCompleted || !reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (line != null)
                {
                    onOutput(line);
                }
            }
        }

        cmd.EndExecute(result);
        _client.Disconnect();

        /*_shell = _client.CreateShellStream("xterm-256color", (uint)cols, (uint)rows, 0, 0, 1024);

        _shell.DataReceived += (sender, e) =>
        {
            var output = Encoding.UTF8.GetString(e.Data);
            onOutput(output);
        };*/
    }

    public void Disconnect()
    {
        _shell?.Close();
        _client?.Disconnect();
        _client?.Dispose();
    }
    
    public void Resize(int cols, int rows)
    {
        _shell?.ChangeWindowSize((uint)cols, (uint)rows, 0, 0);
    }
}