// JudgeWindow.cs
public sealed class JudgeWindow
{
    public bool IsOpen { get; private set; }
    public bool IsExpired { get; private set; }
    public void Open(float duration) { IsOpen = true; }
    public void Tick(float deltaTime) { }
    public void Close() { IsOpen = false; }
}