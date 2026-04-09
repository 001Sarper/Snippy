using System;

namespace Snippy.Models.SSH;

public class TerminalBridge
{
    public int Cols { get; private set; } = 80;
    public int Rows { get; private set; } = 24;
    
    public Action? OnConnect { get; set; }
    public Action<string>? OnInput { get; set; }
    public Action<int, int>? OnResize { get; set; }
    
    public void setSize(int cols, int rows)
    {
        Cols = cols;
        Rows = rows;
        OnResize?.Invoke(cols, rows);
    }
    
    public void connect()
    {
        OnConnect?.Invoke();
    }
}