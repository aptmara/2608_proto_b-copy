using UnityEngine;
using TMPro;

namespace SoroSoro.Events
{
    /// <summary>
    /// ResultDataの内容をTextMeshProを使用してUIに表示するハンドラクラス。
    ///
    /// DisplayDayResult はその日単位の値、DisplayResult は累計値を表示します。
    /// ResultDataは両方で共通の型なので、どちらの値が入っているかは
    /// 呼び出し側（GameClearSceneTransition / GameOver側）の使い分けで担保しています。
    ///
    /// 各TMP_Textは未割り当てでも動作します。
    /// 必要になった枠だけをInspectorで割り当ててください。
    /// </summary>
    public class GameClearResultHandler : MonoBehaviour
    {
        [Header("見出し")]
        [SerializeField, Tooltip("「DAY1　帰宅成功」のような見出しを表示するTMP_Text")]
        private TMP_Text headlineText;

        [SerializeField, Tooltip("日クリア時の見出し書式。{0}に日数が入る")]
        private string dayClearHeadlineFormat = "DAY{0}　帰宅成功";

        [SerializeField, Tooltip("ゲームオーバー時の見出し書式。{0}に日数が入る")]
        private string gameOverHeadlineFormat = "DAY{0}　帰宅失敗";

        [Header("UI References")]
        [SerializeField, Tooltip("到達日数を表示するTMP_Text")]
        private TMP_Text reachedDayText;

        [SerializeField, Tooltip("撃退数を表示するTMP_Text")]
        private TMP_Text repelledCountText;

        [SerializeField, Tooltip("ゲームオーバー理由を表示するTMP_Text")]
        private TMP_Text reasonText;

        [Header("UI References（任意・未割り当て可）")]
        [SerializeField, Tooltip("誤射数を表示するTMP_Text")]
        private TMP_Text wastedCountText;

        [SerializeField, Tooltip("見送り成功数を表示するTMP_Text")]
        private TMP_Text correctCountText;

        [SerializeField, Tooltip("電池残量を表示するTMP_Text")]
        private TMP_Text batteryText;

        [Header("自動反映（GameOverシーン等用）")]
        [SerializeField, Tooltip("Start時にGameEvents.LastGameOverDataを自動で反映するか")]
        private bool autoDisplayGameOverOnStart = false;

        private void Start()
        {
            if (autoDisplayGameOverOnStart && GameEvents.LastGameOverData != null)
            {
                DisplayResult(GameEvents.LastGameOverData);
            }
        }

        /// <summary>
        /// 日クリア時の結果（その日単位の値）をUIに反映します。
        /// ゲームオーバー理由は日クリア時に受け取れないため空にします。
        /// </summary>
        /// <param name="data">表示するResultData</param>
        public void DisplayDayResult(ResultData data)
        {
            if (data == null)
            {
                Debug.LogWarning("ResultDataがnullのため、結果を表示できません。");
                return;
            }

            ApplyHeadline(dayClearHeadlineFormat, data.ReachedDay);
            ApplyCommon(data);

            if (reasonText != null)
            {
                reasonText.text = string.Empty;
            }
        }

        /// <summary>
        /// ゲームオーバー時の結果（累計値）をUIに反映します。
        /// </summary>
        /// <param name="data">表示するResultData</param>
        public void DisplayResult(ResultData data)
        {
            if (data == null)
            {
                Debug.LogWarning("ResultDataがnullのため、結果を表示できません。");
                return;
            }

            ApplyHeadline(gameOverHeadlineFormat, data.ReachedDay);
            ApplyCommon(data);

            if (reasonText != null)
            {
                reasonText.text = ConvertReasonToString(data.Reason);
            }
        }

        /// <summary>
        /// 見出しを書式に従って反映します。
        /// </summary>
        private void ApplyHeadline(string format, int day)
        {
            if (headlineText == null) return;
            if (string.IsNullOrEmpty(format)) return;

            headlineText.text = string.Format(format, day);
        }

        /// <summary>
        /// 日クリアとゲームオーバーで共通の項目を反映します。
        /// </summary>
        private void ApplyCommon(ResultData data)
        {
            if (reachedDayText != null)
            {
                reachedDayText.text = $"{data.ReachedDay} 日";
            }

            if (repelledCountText != null)
            {
                repelledCountText.text = $"{data.RepelledCount} 回";
            }

            if (wastedCountText != null)
            {
                wastedCountText.text = $"{data.TotalWasted} 回";
            }

            if (correctCountText != null)
            {
                correctCountText.text = $"{data.TotalCorrect} 回";
            }

            if (batteryText != null)
            {
                batteryText.text = $"{data.BatteryRemaining:P0}";
            }
        }

        /// <summary>
        /// GameOverReasonを画面表示用のテキストに変換します。
        /// </summary>
        private string ConvertReasonToString(GameOverReason reason)
        {
            switch (reason)
            {
                case GameOverReason.Missed:
                    return "振り返らなかった";
                case GameOverReason.NoShutter:
                    return "撮影できなかった";
                case GameOverReason.NoBattery:
                    return "電池が切れていた";
                default:
                    return reason.ToString();
            }
        }
    }
}