// IDaySequencer.cs
public interface IDaySequencer
{
    bool HasNext { get; }
    SoundEventDefinition Next();
    void Retry();
}