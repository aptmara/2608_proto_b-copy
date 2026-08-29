using UnityEngine;

namespace SoroSoro.Events
{
    /// <summary>
    /// Inspectorおよび実行画面（OnGUI）から各種GameEventsを手動発火させるデバッグ用パネル。
    /// GameManager等のロジック層に一切依存せず、単体で配置して動作します。
    /// </summary>
    public sealed class DebugEventPanel : MonoBehaviour
    {
        [Header("設定")]
        [SerializeField] private float batteryChangedThreshold = 0.01f;

        [Header("デバッグ用選択値")]
        [SerializeField] private int day = 1;
        [SerializeField] private SoundKind soundKind = SoundKind.Ambient;
        [SerializeField] private float judgeWindowDuration = 3.0f;
        [SerializeField] private JudgeResult judgeResult = JudgeResult.Repelled;
        [SerializeField] private float batteryValue = 1.0f;
        [SerializeField] private float progressValue = 0.5f;
        [SerializeField] private GameOverReason gameOverReason = GameOverReason.Missed;

        [Header("リザルト用ダミー値")]
        [SerializeField] private int repelledCount = 3;
        [SerializeField] private int wastedCount = 1;
        [SerializeField] private int correctCount = 5;
        [SerializeField, Range(0f, 1f)] private float batteryRemaining = 0.4f;

        private Vector2 scrollPosition = Vector2.zero;

        private void OnGUI()
        {
            // 操作しやすいよう全体スケール・サイズを拡張
            float panelWidth = 350f;
            float panelHeight = Mathf.Min(Screen.height - 40f, 800f);

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 18,
                fixedHeight = 45f
            };

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter
            };

            GUILayout.BeginArea(new Rect(20, 20, panelWidth, panelHeight), GUI.skin.box);
            GUILayout.Label("<b>Debug Event Panel</b>", labelStyle);
            GUILayout.Space(10);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);

            if (GUILayout.Button("OnDayStarted", buttonStyle))
            {
                GameEvents.RaiseDayStarted(day);
            }
            GUILayout.Space(5);

            if (GUILayout.Button("OnSoundPlayed", buttonStyle))
            {
                GameEvents.RaiseSoundPlayed(soundKind);
            }
            GUILayout.Space(5);

            if (GUILayout.Button("OnJudgeWindowOpened", buttonStyle))
            {
                GameEvents.RaiseJudgeWindowOpened(judgeWindowDuration);
            }
            GUILayout.Space(5);

            if (GUILayout.Button("OnAimStarted", buttonStyle))
            {
                GameEvents.RaiseAimStarted();
            }
            GUILayout.Space(5);

            if (GUILayout.Button("OnAimEnded", buttonStyle))
            {
                GameEvents.RaiseAimEnded();
            }
            GUILayout.Space(5);

            if (GUILayout.Button("OnJudged", buttonStyle))
            {
                GameEvents.RaiseJudged(judgeResult);
            }
            GUILayout.Space(5);

            if (GUILayout.Button("OnBatteryChanged", buttonStyle))
            {
                GameEvents.RaiseBatteryChanged(batteryValue, batteryChangedThreshold);
            }
            GUILayout.Space(5);

            if (GUILayout.Button("OnProgressChanged", buttonStyle))
            {
                GameEvents.RaiseProgressChanged(progressValue);
            }
            GUILayout.Space(5);

            if (GUILayout.Button("OnDayCleared", buttonStyle))
            {
                var clearData = new ResultData
                {
                    ReachedDay = day,
                    RepelledCount = repelledCount,
                    TotalWasted = wastedCount,
                    TotalCorrect = correctCount,
                    BatteryRemaining = batteryRemaining,
                    // Reason は日クリア時には使用しない
                };
                GameEvents.RaiseDayCleared(clearData);
            }
            GUILayout.Space(5);

            // ButtonHandlerを経由せずGameManagerの待機を解除できる。
            // GameClearResultシーンが未完成でも次の日へ進められるため検証が速い。
            if (GUILayout.Button("OnDayClearContinue", buttonStyle))
            {
                GameEvents.RaiseDayClearContinue();
            }
            GUILayout.Space(5);

            if (GUILayout.Button("OnGameOver", buttonStyle))
            {
                var data = new ResultData
                {
                    ReachedDay = day,
                    RepelledCount = repelledCount,
                    TotalWasted = wastedCount,
                    TotalCorrect = correctCount,
                    BatteryRemaining = batteryRemaining,
                    Reason = gameOverReason
                };
                GameEvents.RaiseGameOver(data);
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}