// JudgeWindow.cs
using UnityEngine;

public sealed class JudgeWindow
{
    float remain;

    public bool IsOpen { get; private set; }
    public float Remain => Mathf.Max(0f, remain);
    public float Duration { get; private set; }
    public float NormalizedRemain => Duration <= 0f ? 0f : Remain / Duration;

    public void Open(float duration)
    {
        Duration = duration;
        remain = duration;
        IsOpen = true;
    }

    public void Tick(float deltaTime)
    {
        if (!IsOpen) return;
        remain -= deltaTime;
    }

    public bool IsExpired => IsOpen && remain <= 0f;

    public void Close()
    {
        IsOpen = false;
        remain = 0f;
    }
}