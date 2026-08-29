using UnityEngine;
using TMPro;

namespace SoroSoro.Events
{
    /// <summary>
    /// ResultDataの内容をTextMeshProを使用してUIに表示するハンドラクラス。
    /// </summary>
    public class GameClearResultHandler : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField, Tooltip("到達日数を表示するTMP_Text")]
        private TMP_Text reachedDayText;

        [SerializeField, Tooltip("撃退数を表示するTMP_Text")]
        private TMP_Text repelledCountText;

        [SerializeField, Tooltip("ゲームオーバー理由を表示するTMP_Text")]
        private TMP_Text reasonText;

        /// <summary>
        /// リザルト結果をUIに反映します。
        /// </summary>
        /// <param name="data">表示するResultData</param>
        public void DisplayResult(ResultData data)
        {
            if (data == null)
            {
                Debug.LogWarning("ResultDataがnullのため、結果を表示できません。");
                return;
            }

            if (reachedDayText != null)
            {
                reachedDayText.text = $"{data.ReachedDay} 日";
            }

            if (repelledCountText != null)
            {
                repelledCountText.text = $"{data.RepelledCount} 回";
            }

            if (reasonText != null)
            {
                reasonText.text = ConvertReasonToString(data.Reason);
            }
        }

        /// <summary>
        /// GameOverReasonを画面表示用のテキストに変換します。
        /// </summary>
        private string ConvertReasonToString(GameOverReason reason)
        {
            // 必要に応じて日本語化やカスタムテキストへの変換を行ってください
            return reason.ToString();
        }
    }
}