using UnityEngine;
using TMPro;
using SoroSoro.Events; // GameEventsの参照を追加

namespace SoroSoro.UI
{
    /// <summary>
    /// GameEventsから日数を受け取り、TextMeshProの表示を更新するコンポーネント。
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class DayTextViewer : MonoBehaviour
    {
        [Header("表示設定")]
        [SerializeField, Tooltip("表示フォーマット ({0} に日数が適用されます)")]
        private string format = "DAY {0}";

        private TMP_Text dayText;

        private void Awake()
        {
            dayText = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            GameEvents.OnDayStarted += SetDayText;
        }

        private void OnDisable()
        {
            GameEvents.OnDayStarted -= SetDayText;
        }

        /// <summary>
        /// 受け取った日数を整形してTMP_Textに適用します。
        /// </summary>
        /// <param name="day">開始された日数</param>
        public void SetDayText(int day)
        {
            if (dayText != null)
            {
                dayText.text = string.Format(format, day);
            }
        }
    }
}