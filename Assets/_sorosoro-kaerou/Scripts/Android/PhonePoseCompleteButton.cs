using SoroSoro.Events;
using UnityEngine;

public class PhonePoseCompleteButton : MonoBehaviour
{
    public void OnClick()
    {
        GameEvents.RaisePhonePoseConfirmed();
        AndroidManager.Instance.Gyro.ResetRotation();
    }
}