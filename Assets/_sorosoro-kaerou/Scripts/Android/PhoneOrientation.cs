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

    // 参考表示用の生オイラー角
    // ジンバルロックで暴れるため判定には使わない
    public Vector3 phoneAngle;

    // =========================
    // キャリブレーション用
    // =========================

    // リセット時のYaw角度
    private float yawOffset = 0f;

    // リセット時のPitch角度
    private float pitchOffset = 0f;

    // =========================
    // 生の角度
    // =========================

    // 直近フレームで計算した生のYaw
    // -180〜180
    private float rawYaw;

    // 直近フレームで計算した生のPitch
    private float rawPitch;

    // =========================
    // 現在の相対角度
    // =========================

    // 現在の基準からのYaw角度
    // 0 = 正面
    // 180 = 背後
    // 0〜360
    public float relativeZ;

    // 現在の基準からのPitch角度
    // リセット時 = 0
    public float pitch;

    // =========================
    // 前後判定
    // =========================

    // 反対方向を向いているか
    public bool isBack;

    // =========================
    // スマホの姿勢
    // =========================

    // スマホの姿勢(Unity座標変換後)
    public Quaternion Rotation { get; private set; }

    // =========================
    // フィードバック
    // =========================

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


        // =========================
        // Android座標 → Unity座標
        // =========================

        Quaternion rotation = new Quaternion(
            attitude.x,
            attitude.y,
            -attitude.z,
            -attitude.w
        );

        Rotation = rotation;

        // 参考表示用
        // ジンバルロックで暴れる可能性があるため
        // 判定には使用しない
        phoneAngle = rotation.eulerAngles;


        // =========================
        // Yaw / Pitch計算
        // =========================
        //
        // オイラー角ではなく、スマホのforwardベクトルから計算する。
        //
        // オイラー角分解は途中の軸が90度付近になると
        // ジンバルロックによって他の軸の値と入れ替わったように
        // 暴れるため使用しない。
        // =========================

        Vector3 lookDir = rotation * Vector3.forward;


        // =========================
        // 生Yaw
        // =========================

        rawYaw =
            Mathf.Atan2(lookDir.x, lookDir.y) *
            Mathf.Rad2Deg;


        // =========================
        // 生Pitch
        // =========================

        rawPitch =
            Mathf.Asin(
                Mathf.Clamp(lookDir.z, -1f, 1f)
            ) *
            Mathf.Rad2Deg;


        // =========================
        // リセット基準からのYaw
        // =========================

        relativeZ =
            Mathf.Repeat(
                rawYaw - yawOffset,
                360f
            );


        // =========================
        // リセット基準からのPitch
        // =========================

        pitch =
            rawPitch - pitchOffset;


        // =========================
        // 前 / 後ろ判定
        // =========================

        if (!isBack)
        {
            if (135 < relativeZ && relativeZ < 225)
            {
                isBack = true;
            }
        }
        else
        {
            if (315 < relativeZ || relativeZ < 45)
            {
                isBack = false;

                if (feedback != null && feedback.IsLightOn)
                {
                    feedback.SetLight(false);
                }
            }
        }


        // =========================
        // 後ろを向いているときのフィードバック
        // =========================

        if (isBack)
        {
            feedback = AndroidManager.Instance.Feedback;

            if (feedback != null)
            {
                if (!feedback.IsLightOn)
                {
                    feedback.SetLight(true);
                }

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
                $"(raw X:{phoneAngle.x:F0} " +
                $"Y:{phoneAngle.y:F0} " +
                $"Z:{phoneAngle.z:F0})";
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
        // 現在のYawを0°の基準にする
        yawOffset = rawYaw;

        // 現在のPitchを0°の基準にする
        pitchOffset = rawPitch;

        // リセット直後の表示値を明示的に0にする
        relativeZ = 0f;
        pitch = 0f;

        // 必要なら前後判定もリセット
        isBack = false;

        // リセット時にライトが点いていたら消す
        if (feedback != null && feedback.IsLightOn)
        {
            feedback.SetLight(false);
        }
    }
}
