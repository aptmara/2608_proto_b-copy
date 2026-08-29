using SorosoroKaerou;
using SoroSoro.Events;
using UnityEngine;

public class AndroidPlayerInput : MonoBehaviour, IPlayerInput
{
    private PhoneGyro gyro;
    private AndroidTorch torch;

    public float TurnAxis => gyro.relativeZ;
    public bool IsTurnedBack => gyro.isBack;

    public bool ShutterDown { get; private set; }

    private void Awake()
    {
        gyro = GetComponent<PhoneGyro>();
        torch = GetComponent<AndroidTorch>();
    }

    private void OnEnable()
    {
        GameEvents.OnShutterRequested += TriggerShutter;
    }

    private void OnDisable()
    {
        GameEvents.OnShutterRequested -= TriggerShutter;
    }

    // 読み取った側（GameManager）が明示的に呼ぶ。毎フレーム自動リセットしない。
    public void ConsumeShutter()
    {
        ShutterDown = false;
    }

    private void TriggerShutter()
    {
        ShutterDown = true;
    }
}