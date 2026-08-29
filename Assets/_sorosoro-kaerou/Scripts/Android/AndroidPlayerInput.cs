using SorosoroKaerou;
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

    private void Update()
    {
        // 毎フレームリセット
        ShutterDown = false;
    }

    public void TriggerShutter()
    {
        ShutterDown = true;
    }
}