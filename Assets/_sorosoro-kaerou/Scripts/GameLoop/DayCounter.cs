// DayCounter.cs
public sealed class DayCounter
{
    readonly DayConfig[] configs;
    int index;
    int overflowDays; // 最終Dayの設定に到達した後、Advanceされた回数

    public DayCounter(DayConfig[] configs)
    {
        this.configs = configs;
    }

    // 最終Day到達後はConfigのdayに加算分を足して返す
    public int CurrentDay => CurrentConfig != null ? CurrentConfig.day + overflowDays : 0;

    public DayConfig CurrentConfig =>
        configs != null && index >= 0 && index < configs.Length ? configs[index] : null;

    public bool HasNext => configs != null && index + 1 < configs.Length;

    public void Advance()
    {
        if (HasNext)
        {
            index++;
        }
        else
        {
            // 最終Configのまま同じ設定を使い続け、Dayの数字だけ加算する
            overflowDays++;
        }
    }

    public void Reset()
    {
        index = 0;
        overflowDays = 0;
    }
}