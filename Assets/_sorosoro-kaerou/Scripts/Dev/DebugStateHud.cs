// DebugStateHud.cs
// GameEventsを購読するだけの検証用HUD。GameManagerやScene構成には一切手を加えない。
// シーンに空のGameObjectを1つ作ってアタッチするだけで動く。
// UNITY_EDITOR / DEVELOPMENT_BUILD限定。本番ビルドには含まれない。
using UnityEngine;
using SoroSoro.Events;

public sealed class DebugStateHud : MonoBehaviour
{
    int day;
    float progress;
    float battery = 1f;
    bool isAiming;
    string lastSound = "-";
    string lastJudged = "-";
    string gameOverInfo = "-";

    bool judgeOpen;
    float judgeDuration;
    float judgeOpenedAt;

    void OnEnable()
    {
        GameEvents.OnDayStarted += HandleDayStarted;
        GameEvents.OnProgressChanged += HandleProgress;
        GameEvents.OnBatteryChanged += HandleBattery;
        GameEvents.OnSoundPlayed += HandleSoundPlayed;
        GameEvents.OnJudgeWindowOpened += HandleJudgeOpened;
        GameEvents.OnJudged += HandleJudged;
        GameEvents.OnAimStarted += HandleAimStarted;
        GameEvents.OnAimEnded += HandleAimEnded;
        GameEvents.OnDayCleared += HandleDayCleared;
        GameEvents.OnGameOver += HandleGameOver;
    }

    void OnDisable()
    {
        GameEvents.OnDayStarted -= HandleDayStarted;
        GameEvents.OnProgressChanged -= HandleProgress;
        GameEvents.OnBatteryChanged -= HandleBattery;
        GameEvents.OnSoundPlayed -= HandleSoundPlayed;
        GameEvents.OnJudgeWindowOpened -= HandleJudgeOpened;
        GameEvents.OnJudged -= HandleJudged;
        GameEvents.OnAimStarted -= HandleAimStarted;
        GameEvents.OnAimEnded -= HandleAimEnded;
        GameEvents.OnDayCleared -= HandleDayCleared;
        GameEvents.OnGameOver -= HandleGameOver;
    }

    void HandleDayStarted(int d) { day = d; gameOverInfo = "-"; lastJudged = "-"; }
    void HandleProgress(float p) => progress = p;
    void HandleBattery(float b) => battery = b;
    void HandleSoundPlayed(SoundKind k) { lastSound = k.ToString(); judgeOpen = false; }
    void HandleJudgeOpened(float duration) { judgeDuration = duration; judgeOpenedAt = Time.time; judgeOpen = true; }
    void HandleJudged(JudgeResult r) { lastJudged = r.ToString(); judgeOpen = false; }
    void HandleAimStarted() => isAiming = true;
    void HandleAimEnded() => isAiming = false;
    void HandleDayCleared(ResultData data) => lastJudged = $"DayCleared(day{data.ReachedDay} repelled={data.RepelledCount})";
    void HandleGameOver(ResultData data)
        => gameOverInfo = $"day={data.ReachedDay} repelled={data.RepelledCount} reason={data.Reason}";

    void OnGUI()
    {
        // 画面解像度に依存せず必ず全項目が収まるよう、高さを動的に確保する
        float areaWidth = 1000f;
        float areaHeight = 800f;

        GUIStyle title = new GUIStyle(GUI.skin.label) { fontSize = 64, fontStyle = FontStyle.Bold };
        GUIStyle label = new GUIStyle(GUI.skin.label) { fontSize = 64, wordWrap = false };

        GUILayout.BeginArea(new Rect(10, 10, areaWidth, areaHeight), GUI.skin.box);
        GUILayout.Label("Debug State HUD", title);
        GUILayout.Label($"Day: {day}", label);
        GUILayout.Label($"Progress: {progress:P0}", label);
        GUILayout.Label($"Battery: {battery:P0}", label);
        GUILayout.Label($"Aiming: {isAiming}", label);
        GUILayout.Label($"LastSound: {lastSound}", label);

        if (judgeOpen)
        {
            float remain = Mathf.Max(0f, judgeDuration - (Time.time - judgeOpenedAt));
            GUILayout.Label($"JudgeWindow: OPEN 残{remain:F1}s", label);
        }
        else
        {
            GUILayout.Label("JudgeWindow: closed", label);
        }

        GUILayout.Label($"LastJudged: {lastJudged}", label);
        GUILayout.Label($"GameOver: {gameOverInfo}", label);
        GUILayout.EndArea();
    }
}