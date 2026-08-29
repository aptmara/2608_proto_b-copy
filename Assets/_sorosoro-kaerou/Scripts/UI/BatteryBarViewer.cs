using UnityEngine;
using UnityEngine.UI;
using TMPro; // TMP_Text の使用に必要な名前空間
using SoroSoro.Events;

public class BatteryBarViewer : MonoBehaviour
{
    [SerializeField] private Image batteryBar;
    [SerializeField] private TMP_Text batteryText; // 追加: テキスト表示用
    [SerializeField] private float lerpSpeed = 10f; // 追加: 補間速度

    private float targetValue = 1f;
    private float currentValue = 1f;

    private void OnEnable()
    {
        // イベントの購読登録
        GameEvents.OnBatteryChanged += OnBatteryChanged;
    }

    private void OnDisable()
    {
        // イベントの購読解除
        GameEvents.OnBatteryChanged -= OnBatteryChanged;
    }

    private void Update()
    {
        // 現在値を目標値に向かって Lerp で滑らかに補間
        currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * lerpSpeed);

        // バッテリーバーの Scale X を更新
        if (batteryBar != null)
        {
            Vector3 currentScale = batteryBar.rectTransform.localScale;
            batteryBar.rectTransform.localScale = new Vector3(currentValue, currentScale.y, currentScale.z);
        }

        // テキスト表示を更新（例: 0% ～ 100% 表示）
        if (batteryText != null)
        {
            batteryText.text = $"{Mathf.RoundToInt(currentValue * 100f)}%";
        }
    }

    /// <summary>
    /// イベント受信時のハンドラ
    /// </summary>
    private void OnBatteryChanged(float batteryValue)
    {
        // 目標値を更新 (0 ～ 1 の範囲にクランプ)
        targetValue = Mathf.Clamp01(batteryValue);
    }
}