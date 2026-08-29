// RandomSequencer.cs
using System;

public sealed class RandomSequencer : IDaySequencer
{
    readonly SoundEventDefinition[] anomalies;
    readonly SoundEventDefinition[] ambients;
    readonly DayConfig config;
    readonly Random random;
    int firedCount;

    public RandomSequencer(DayConfig config, Random random)
    {
        this.config = config;
        this.random = random;
        
        var pool = config.pool ?? Array.Empty<SoundEventDefinition>();
        anomalies = Array.FindAll(pool, x => x != null && x.kind == SoundKind.Anomaly);
        ambients = Array.FindAll(pool, x => x != null && x.kind == SoundKind.Ambient);
    }

    public bool HasNext => firedCount < config.eventCount;

    public SoundEventDefinition Next()
    {
        if (!HasNext) return null;

        firedCount++;
        bool isAnomaly = random.NextDouble() < config.anomalyRatio;
        var selectPool = isAnomaly ? anomalies : ambients;
        
        if (selectPool.Length == 0) selectPool = isAnomaly ? ambients : anomalies;
        if (selectPool.Length == 0) return null;
        
        return selectPool[random.Next(selectPool.Length)];
    }

    public void Retry() { }
}