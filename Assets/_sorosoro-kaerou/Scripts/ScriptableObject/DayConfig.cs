// DayConfig.cs
using UnityEngine;

[CreateAssetMenu(fileName = "DayConfig", menuName = "SoroSoro/DayConfig")]
public sealed class DayConfig : ScriptableObject
{
    [Tooltip("何日目の設定か（Day 0を含む）")]
    public int day = 1;

    [Tooltip("正面歩行の必要累計時間（この秒数歩くと次の日へ進む）")]
    [Min(1f)]
    public float requiredWalkSeconds = 60f;

    [Tooltip("その日に発生させる音イベントの総数")]
    [Min(0)]
    public int eventCount = 5;

    [Tooltip("音イベント抽選時の異常音の割合（0.0〜1.0）")]
    [Range(0f, 1f)]
    public float anomalyRatio = 0.4f;

    [Tooltip("音が鳴ってから判定が終了するまでの実時間猶予（秒）")]
    [Min(0.1f)]
    public float judgeWindowDuration = 3f;

    [Tooltip("判定失敗時にゲームオーバーとするか（Day 0はfalse）")]
    public bool allowGameOver = true;

    [Tooltip("撃退数をスコアとして加算するか（Day 0はfalse）")]
    public bool countScore = true;

    [Tooltip("Day 0専用の固定イベント列（※本編用Configでは空配列のままでよい）")]
    public SoundEventDefinition[] fixedSequence;

    [Tooltip("その日に出現する音イベントの抽選プール")]
    public SoundEventDefinition[] pool;
}