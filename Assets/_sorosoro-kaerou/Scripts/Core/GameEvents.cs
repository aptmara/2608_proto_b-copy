using System;
using UnityEngine;

namespace SoroSoro.Events
{
    /// <summary>
    /// UI層や各演出コンポーネントへゲーム状態の変化を一方向に通知するイベント集約クラス。
    /// C#標準の static event を採用しているため、UI層はGameManagerの実体を参照（DI）することなく、
    /// クラス名指定で直接イベントを購読・解除できます。
    ///
    /// 例外的に OnDayClearContinue と OnShutterRequested だけはUI層→ロジック層への逆方向の通知です。
    /// OnDayClearContinueはリザルト画面を閉じた事実を、OnShutterRequestedはシャッター演出内の
    /// 撮影確定タイミングをGameManagerへ伝えるためだけに使い、他の用途には使いません。
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

        // 帰宅時（引数: その日単位のリザルトデータ）
        public static event Action<ResultData> OnDayCleared;

        // 日クリアリザルトを閉じた時（UI層→ロジック層への逆方向通知）
        public static event Action OnDayClearContinue;

        // シャッター演出内で撮影が確定した時（UI層→ロジック層への逆方向通知）
        public static event Action OnShutterRequested;

        // ゲームオーバー時（引数: 累計のリザルトデータ）
        public static event Action<ResultData> OnGameOver;
        
        // 撮影時の硬直(演出用)
        public static event Action OnStagingFinished;

        // 音イベント発火時（種別問わず）に発火。判定確定まで正体不明のまま近づいてくる演出の起点。
        public static event Action OnGhostAppeared;

        // 撮影が成立したときに発火。写っていなければ null（環境音で未設定の場合など）。
        public static event Action<Sprite> OnPhotoCaptured;
        
        // 歩き音
        public static event Action<bool> OnWalkingChanged;

        // 端末ライト（トーチ）のON/OFF切り替え時。演出側で画面の明るさ表現に使う。
        public static event Action<bool> OnLightOnChanged;

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
            OnDayClearContinue = null;
            OnGameOver = null;
            OnShutterRequested = null;

            // Enter Play Mode Options でDomain Reloadを切っていると前回のPlayの値が残るため必ず戻す
            lastRaisedBattery = -1f;
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
        public static void RaiseDayCleared(ResultData data) => OnDayCleared?.Invoke(data);
        public static void RaiseDayClearContinue() => OnDayClearContinue?.Invoke();
        public static void RaiseGameOver(ResultData data) => OnGameOver?.Invoke(data);
        public static void RaiseShutterRequested() => OnShutterRequested?.Invoke();

        public static void RaiseStagingFinished() => OnStagingFinished?.Invoke();

        public static void RaiseGhostAppeared() => OnGhostAppeared?.Invoke();

        public static void RaisePhotoCaptured(Sprite ghost) => OnPhotoCaptured?.Invoke(ghost);

        public static void RaiseWalkingChanged(bool walking) => OnWalkingChanged?.Invoke(walking);

        public static void RaiseLightOnChanged(bool on) => OnLightOnChanged?.Invoke(on);
    }
}