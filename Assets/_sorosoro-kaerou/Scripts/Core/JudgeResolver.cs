// JudgeResolver.cs
public sealed class JudgeResolver
{
    // 引数以外の状態を持たず何も書き換えない純粋関数
    public JudgeResult Resolve(in JudgeContext ctx)
    {
        if (ctx.kind == SoundKind.Anomaly)
        {
            if (!ctx.didTurn) return JudgeResult.Missed;
            if (!ctx.didShutter) return JudgeResult.Missed;
            if (ctx.batteryWasEmptyOnShutter) return JudgeResult.Missed;
            return JudgeResult.Repelled;
        }

        // Ambient分岐には電池切れによるMissed（GameOver）が存在しない
        if (!ctx.didTurn) return JudgeResult.Correct;
        if (!ctx.didShutter) return JudgeResult.Correct;
        return JudgeResult.Wasted;
    }

    public GameOverReason ResolveReason(in JudgeContext ctx)
    {
        if (!ctx.didTurn) return GameOverReason.Missed;
        if (!ctx.didShutter) return GameOverReason.NoShutter;
        return GameOverReason.NoBattery;
    }
}