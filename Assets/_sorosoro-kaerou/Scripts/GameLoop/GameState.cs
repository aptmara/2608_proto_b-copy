// GameState.cs
using SorosoroKaerou;

public sealed class GameState
{
    public PhaseKind Phase { get; private set; } = PhaseKind.Title;
    public bool IsTurnedBack { get; private set; }
    public bool IsWalkingForward => !IsTurnedBack && Phase is PhaseKind.Walking or PhaseKind.Judging;
    public SoundEventDefinition CurrentEvent { get; private set; }
    public bool HasAimedThisEvent { get; private set; }

    public void SetPhase(PhaseKind next)
    {
        if (Phase == next) return;
        Phase = next;
    }

    public void SyncInput(IPlayerInput input)
    {
        IsTurnedBack = input.IsTurnedBack;
        if (IsTurnedBack) HasAimedThisEvent = true;
    }

    public void SetCurrentEvent(SoundEventDefinition def)
    {
        CurrentEvent = def;
        HasAimedThisEvent = false;
    }

    public void ClearCurrentEvent()
    {
        CurrentEvent = null;
        HasAimedThisEvent = false;
    }
}