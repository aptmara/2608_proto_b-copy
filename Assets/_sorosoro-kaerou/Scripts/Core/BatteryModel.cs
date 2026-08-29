using UnityEngine;

public sealed class BatteryModel
{
    readonly GameBalanceConfig config;
    float remain;

    public BatteryModel(GameBalanceConfig config)
    {
        this.config = config;
        if (config != null)
        {
            remain = config.batteryCapacity;
        }
    }

    public float Normalized => (config == null || config.batteryCapacity <= 0f)
        ? 0f
        : Mathf.Clamp01(remain / config.batteryCapacity);

    public bool IsEmpty => remain <= 0f;

    public void DrainLight(float deltaTime)
    {
        if (IsEmpty || config == null) return;
        remain = Mathf.Max(0f, remain - config.lightDrainPerSecond * deltaTime);
    }

    public bool TryConsumeFlash()
    {
        if (config == null || remain < config.flashCost) return false;
        remain -= config.flashCost;
        return true;
    }

    public void Refill()
    {
        if (config != null)
        {
            remain = config.batteryCapacity;
        }
    }
}