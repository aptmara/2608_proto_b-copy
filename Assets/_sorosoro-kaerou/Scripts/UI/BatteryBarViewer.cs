using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SoroSoro.Events;

public class BatteryBarViewer : MonoBehaviour
{
    [SerializeField] private Image batteryBar;
    [SerializeField] private TMP_Text batteryText;
    [SerializeField] private float lerpSpeed = 10f;

    private float targetValue = 1f;
    private float currentValue = 1f;

    private void OnEnable()
    {
        GameEvents.OnBatteryChanged += OnBatteryChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnBatteryChanged -= OnBatteryChanged;
    }

    private void Update()
    {
        currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * lerpSpeed);

        if (batteryBar != null)
        {
            Vector3 currentScale = batteryBar.rectTransform.localScale;
            batteryBar.rectTransform.localScale = new Vector3(currentValue, currentScale.y, currentScale.z);
        }

        if (batteryText != null)
        {
            batteryText.text = $"{Mathf.RoundToInt(currentValue * 100f)}%";
        }
    }

    private void OnBatteryChanged(float batteryValue)
    {
        targetValue = Mathf.Clamp01(batteryValue);
    }
}