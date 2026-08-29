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

    // スマホの生の角度
    public Vector3 phoneAngle;

    // リセット時のZ角度
    private float ZOffset = 0f;

    // 現在の基準からのZ角度
    public float relativeZ;

    // 反対方向を向いているか
    public bool isBack;

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


        // =========================
        // オイラー角
        // =========================

        phoneAngle = rotation.eulerAngles;


        // =========================
        // リセット位置を基準にしたY角度
        // =========================

        relativeZ = Mathf.Repeat(
            phoneAngle.z - ZOffset,
            360f
        );


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
                $"Z : {phoneAngle.z:F1}°\n" +
                $"Z Offset : {ZOffset:F1}°\n" +
                $"Relative Z : {relativeZ:F1}°"
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
        ZOffset = phoneAngle.z;
    }
}