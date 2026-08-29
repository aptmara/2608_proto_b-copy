// TutorialSequencer.cs
// Day 0専用。固定順序でイベントを供給し、失敗時（Retry）は同じイベントをもう一度返す。
// indexを進めるのはNext()の中だけに限定する（Retry後の二重再生を防ぐため）。
using UnityEngine;

public sealed class TutorialSequencer : IDaySequencer
{
    readonly SoundEventDefinition[] sequence;
    int index;
    bool shouldRetry;

    public TutorialSequencer(SoundEventDefinition[] sequence)
    {
        this.sequence = sequence ?? System.Array.Empty<SoundEventDefinition>();
    }

    public bool HasNext => shouldRetry ? index > 0 : index < sequence.Length;

    public SoundEventDefinition Next()
    {
        if (sequence.Length == 0) return null;

        if (shouldRetry)
        {
            shouldRetry = false;
            // indexは進めず、直前に返したものをもう一度返す
            return sequence[Mathf.Clamp(index - 1, 0, sequence.Length - 1)];
        }

        if (index >= sequence.Length) return null;

        var def = sequence[index];
        index++;
        return def;
    }

    // 失敗時に呼ばれる。フラグを立てるだけでindexは触らない
    public void Retry() => shouldRetry = true;
}
