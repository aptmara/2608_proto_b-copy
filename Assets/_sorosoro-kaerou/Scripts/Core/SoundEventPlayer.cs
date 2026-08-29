// SoundEventPlayer.cs
using System;

public sealed class SoundEventPlayer
{
    readonly GameBalanceConfig balance;
    readonly Random random;
    IDaySequencer sequencer;
    float cooldownTimer;

    public SoundEventPlayer(GameBalanceConfig balance, Random random)
    {
        this.balance = balance;
        this.random = random;
    }

    public void SetSequencer(IDaySequencer sequencer)
    {
        this.sequencer = sequencer;
        ResetCooldown();
    }

    void ResetCooldown()
    {
        float min = balance.eventCooldownMin;
        float max = balance.eventCooldownMax;
        cooldownTimer = min + (float)random.NextDouble() * (max - min);
    }

    public void Tick(float deltaTime, bool isWalkingForward)
    {
        if (!isWalkingForward) return;
        cooldownTimer -= deltaTime;
    }

    public bool CanFire(float remainToDayEnd)
    {
        if (sequencer == null || !sequencer.HasNext) return false;
        if (cooldownTimer > 0f) return false;
        if (remainToDayEnd < balance.noEventBeforeEnd) return false;
        return true;
    }

    public SoundEventDefinition Fire()
    {
        ResetCooldown();
        return sequencer.Next();
    }
}