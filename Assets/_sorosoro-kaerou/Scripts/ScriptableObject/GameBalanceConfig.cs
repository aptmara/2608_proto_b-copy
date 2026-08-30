// GameBalanceConfig.cs
using UnityEngine;

[CreateAssetMenu(fileName = "GameBalanceConfig", menuName = "SoroSoro/GameBalanceConfig")]
public sealed class GameBalanceConfig : ScriptableObject
{
    [Header("電池")]
    [Tooltip("1日あたりの電池の総容量（上限基準値）")]
    public float batteryCapacity = 100f;

    [Tooltip("振り返ってライトを点灯している間、毎秒消費する電池量")]
    public float lightDrainPerSecond = 8f;

    [Tooltip("撮影1回あたりに一括消費する電池量")]
    public float flashCost = 15f;

    [Header("イベント")]
    [Tooltip("判定終了から次の音イベント抽選までの間隔（最小）")]
    public float eventCooldownMin = 4f;

    [Tooltip("判定終了から次の音イベント抽選までの間隔（最大）")]
    public float eventCooldownMax = 8f;

    [Tooltip("日の終了数秒前は新規イベントを発生させないための禁止時間（秒）")]
    public float noEventBeforeEnd = 3f;
    
    [Header("演出")]
    [Tooltip("撮影後、演出を見せるために進行を停止する時間（秒）。0にすると演出待機は完全に無効化され、導入前と同じ挙動に戻る")]
    public float stagingWaitTime = 1.2f;

    [Header("ペナルティ")]
    [Tooltip("環境音に対して空振り撮影した際の硬直時間（秒）")]
    public float wastedStunTime = 0.4f;

    [Tooltip("後ろを向き続けてからイベントを発火するまでの時間（秒）")]
    public float lookBackDurationThreshold = 2f;
    
    [Header("幽霊抽選プール")]
    public Sprite[] fallbackGhostSprites;
}