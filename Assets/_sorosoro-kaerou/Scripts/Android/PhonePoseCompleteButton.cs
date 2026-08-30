using SoroSoro.Events;
using UnityEngine;

/// <summary>
/// スマホ姿勢キャリブレーション完了ボタン。
/// </summary>
public class PhonePoseCompleteButton : MonoBehaviour
{
    /// <summary>
    /// ボタン押下時のコールバック。
    /// </summary>
    public void OnClick()
    {
        GameEvents.RaisePhonePoseConfirmed();
        if (AndroidManager.Instance != null && AndroidManager.Instance.Gyro != null)
        {
            AndroidManager.Instance.Gyro.ResetRotation();
        }
    }
}