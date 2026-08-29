// DayCounter.cs
public sealed class DayCounter
{
    readonly DayConfig[] configs;
    int index;

    public DayCounter(DayConfig[] configs)
    {
        this.configs = configs;
    }

    public int CurrentDay => CurrentConfig != null ? CurrentConfig.day : 0;

    public DayConfig CurrentConfig =>
        configs != null && index >= 0 && index < configs.Length ? configs[index] : null;

    public bool HasNext => configs != null && index + 1 < configs.Length;

    public void Advance()
    {
        if (HasNext) index++;
    }

    public void Reset() => index = 0;
}