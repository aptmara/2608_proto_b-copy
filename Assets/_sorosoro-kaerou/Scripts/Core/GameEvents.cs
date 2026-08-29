using System;
using UnityEngine;

namespace SoroSoro.Events
{
    /// <summary>
    /// UI層や各演出コンポーネントへゲーム状態の変化を一方向に通知するイベント集約クラス。
    /// C#標準の static event を採用しているため、UI層はGameManagerの実体を参照（DI）することなく、
    /// クラス名指定で直接イベントを購読・解除できます。
    /// </summary>
    public static class GameEvents
    {
        // 日の開始時（引数: 日数）
        public static event Action<int> OnDayStarted;

        // 音イベント発生時（引数: 音の種別）
        public static event Action<SoundKind> OnSoundPlayed;

        // 判定受付の開始時（引数: 制限時間秒数）
        public static event Action<float> OnJudgeWindowOpened;

        // 振り返り開始時（不変条件2により引数なし）
        public static event Action OnAimStarted;

        // 正面復帰時・判定確定時（不変条件2により引数なし）
        public static event Action OnAimEnded;

        // 判定確定時（引数: 判定結果）
        public static event Action<JudgeResult> OnJudged;

        // 電池残量変化時（引数: 0.0〜1.0の正規化残量）
        public static event Action<float> OnBatteryChanged;

        // 進行度変化時（引数: 0.0〜1.0の正規化進行度）
        public static event Action<float> OnProgressChanged;

        // 帰宅時（引数: 日数）
        public static event Action<int> OnDayCleared;

        // ゲームオーバー時（引数: リザルトデータ）
        public static event Action<ResultData> OnGameOver;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            OnDayStarted = null;
            OnSoundPlayed = null;
            OnJudgeWindowOpened = null;
            OnAimStarted = null;
            OnAimEnded = null;
            OnJudged = null;
            OnBatteryChanged = null;
            OnProgressChanged = null;
            OnDayCleared = null;
            OnGameOver = null;
        }

        public static void RaiseDayStarted(int day) => OnDayStarted?.Invoke(day);
        public static void RaiseSoundPlayed(SoundKind kind) => OnSoundPlayed?.Invoke(kind);
        public static void RaiseJudgeWindowOpened(float duration) => OnJudgeWindowOpened?.Invoke(duration);
        public static void RaiseAimStarted() => OnAimStarted?.Invoke();
        public static void RaiseAimEnded() => OnAimEnded?.Invoke();
        public static void RaiseJudged(JudgeResult result) => OnJudged?.Invoke(result);

        private static float lastRaisedBattery = -1f;

        public static void RaiseBatteryChanged(float normalized, float threshold = 0.01f)
        {
            if (Mathf.Abs(normalized - lastRaisedBattery) < threshold && normalized > 0f && normalized < 1f)
            {
                return;
            }
            lastRaisedBattery = normalized;
            OnBatteryChanged?.Invoke(normalized);
        }

        public static void RaiseProgressChanged(float normalized) => OnProgressChanged?.Invoke(normalized);
        public static void RaiseDayCleared(int day) => OnDayCleared?.Invoke(day);
        public static void RaiseGameOver(ResultData data) => OnGameOver?.Invoke(data);
    }
}