using SorosoroKaerou;
using TMPro;
using UnityEngine;

public class PhoneGyro : MonoBehaviour
{
    private Gyroscope gyro;

    [Header("Cube")]
    [SerializeField] private GameObject cube;

    [Header("TMP")]
    [SerializeField] private TMP_Text rotationText;
    [SerializeField] private TMP_Text directionText;
    [SerializeField] private TMP_Text feedbackText;

    // 参考表示用の生オイラー角(ジンバルロックで暴れるため判定には使わない)
    public Vector3 phoneAngle;

    // リセット時のYaw角度
    private float yawOffset = 0f;

    // 直近フレームで計算した生のYaw(-180〜180、キャリブレーション前)
    private float rawYaw;

    // 現在の基準からのYaw角度(0=正面, 180=背後の連続角度。isBack判定に使用)
    public float relativeZ;

    // 上下の傾き(度。上向きが正、範囲はおよそ-90〜90)
    public float pitch;

    // 反対方向を向いているか
    public bool isBack;

    // スマホの姿勢(Unity座標変換後)
    public Quaternion Rotation { get; private set; }

    // フィードバック用のインターフェース
    private IFeedbackPresenter feedback;


    void Start()
    {
        // =========================
        // 横画面固定
        // =========================

        Screen.orientation = ScreenOrientation.LandscapeLeft;

        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;


        // =========================
        // ジャイロ
        // =========================

        gyro = Input.gyro;
        gyro.enabled = true;
    }


    void Update()
    {
        // =========================
        // ジャイロから姿勢を取得
        // =========================

        Quaternion attitude = gyro.attitude;

        // Android座標 → Unity座標
        Quaternion rotation = new Quaternion(
            attitude.x,
            attitude.y,
            -attitude.z,
            -attitude.w
        );

        Rotation = rotation;
        phoneAngle = rotation.eulerAngles; // 参考表示用のみ(判定には使わない)


        // =========================
        // Yaw / Pitchをオイラー角ではなくベクトル射影(Atan2)で求める。
        // オイラー角分解は途中の軸が90度付近になるとジンバルロックで
        // 他の軸の値と入れ替わったように暴れるため、傾き操作(ピッチ)が
        // 絡むこのアプリでは使えない。
        // =========================

        // Vector3.upは体を振り向く回転(Yaw)の回転軸そのものに一致していて
        // Yawに反応しなかったため、それと直交するVector3.forwardを使う
        Vector3 lookDir = rotation * Vector3.forward; // スマホが向いている方向(要検証軸)

        // Yaw/PitchがY・Zで入れ替わって見えたため、ペアリングをx-y(Yaw) / z(Pitch)に変更
        rawYaw = Mathf.Atan2(lookDir.x, lookDir.y) * Mathf.Rad2Deg;
        pitch = Mathf.Asin(Mathf.Clamp(lookDir.z, -1f, 1f)) * Mathf.Rad2Deg;

        relativeZ = Mathf.Repeat(rawYaw - yawOffset, 360f);


        // =========================
        // 前 / 後ろ判定
        // =========================

        if(!isBack)
        {
            if ( 135 < relativeZ && relativeZ < 225)
            {
                isBack = true;
            }
        }
        else
        {
            if (315 < relativeZ || relativeZ < 45)
            {
                isBack = false;
                if (feedback.IsLightOn)
                    feedback.SetLight(false);
            }
        }

        if (isBack)
        {
            feedback = AndroidManager.Instance.Feedback;
            if (feedback != null)
            {
                if(!feedback.IsLightOn)
                    feedback.SetLight(true);
                feedback.Vibrate();
            }
        }

        // =========================
        // TMP：ローテーション表示
        // =========================

        if (rotationText != null)
        {
            rotationText.text =
                $"Yaw : {relativeZ:F1}°\n" +
                $"Pitch : {pitch:F1}°\n" +
                $"(raw X:{phoneAngle.x:F0} Y:{phoneAngle.y:F0} Z:{phoneAngle.z:F0})"
                ;
        }


        // =========================
        // TMP：前 / 後ろ
        // =========================

        if (directionText != null)
        {
            directionText.text =
                $"反対向き : {isBack.ToString().ToUpper()}";
        }

        // =========================
        // Cubeをスマホと同じ向きにする
        // =========================

        if (cube != null)
        {
            cube.transform.rotation = rotation;
        }
    }


    // ==================================================
    // 現在の向きを0°にリセット
    // ==================================================

    public void ResetRotation()
    {
        yawOffset = rawYaw;
    }
}
