using TMPro;
using UnityEngine;

namespace SoroSoro.Events
{
    public sealed class DayTextViewer : MonoBehaviour
    {
        [Header("参照")]
        [SerializeField] private TMP_Text dayText;

        [Header("表示設定")]
        [SerializeField] private string format = "DAY {0}";
        [SerializeField] private int initialDay = 1;

        private void Awake()
        {
            // アタッチされている TMP_Text を自動取得
            if (dayText == null)
            {
                dayText = GetComponent<TMP_Text>();
            }

            // 初期化時に Day を表示
            UpdateDayText(initialDay);
        }

        private void OnEnable()
        {
            GameEvents.OnDayStarted += HandleDayStarted;
        }

        private void OnDisable()
        {
            GameEvents.OnDayStarted -= HandleDayStarted;
        }

        private void HandleDayStarted(int day)
        {
            UpdateDayText(day);
        }

        private void UpdateDayText(int day)
        {
            if (dayText != null)
            {
                dayText.text = string.Format(format, day);
            }
        }
    }
}