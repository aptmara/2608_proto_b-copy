// GameBalanceConfig.cs
using UnityEngine;

[CreateAssetMenu(fileName = "GameBalanceConfig", menuName = "SoroSoro/GameBalanceConfig")]
public sealed class GameBalanceConfig : ScriptableObject
{
    [Tooltip("1日あたりの電池の総容量（上限基準値）")]
    public float batteryCapacity = 100f;

    [Tooltip("振り返ってライトを点灯している間、毎秒消費する電池量")]
    public float lightDrainPerSecond = 8f;

    [Tooltip("撮影1回あたりに一括消費する電池量")]
    public float flashCost = 15f;

    [Tooltip("環境音に対して空振り撮影した際の硬直時間（秒）")]
    public float wastedStunTime = 0.4f;

    [Tooltip("判定終了から次の音イベント抽選までの間隔（秒）")]
    public float eventCooldown = 4f;

    [Tooltip("日の終了数秒前は新規イベントを発生させないための禁止時間（秒）")]
    public float noEventBeforeEnd = 3f;
}