// DayProgress.cs
using UnityEngine;

public sealed class DayProgress
{
    float elapsed;
    float required = 1f;

    public float Normalized => Mathf.Clamp01(elapsed / required);
    public bool IsCompleted => elapsed >= required;

    public void Tick(float deltaTime, bool isWalkingForward)
    {
        if (!isWalkingForward) return;
        elapsed += deltaTime;
    }

    public void Reset(float requiredSeconds)
    {
        elapsed = 0f;
        required = Mathf.Max(0.01f, requiredSeconds);
    }
}