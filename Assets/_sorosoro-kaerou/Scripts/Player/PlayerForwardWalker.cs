// PlayerForwardWalker.cs
using UnityEngine;

namespace SorosoroKaerou
{
    /// <summary>
    /// AndroidManager(IPlayerInput / IFeedbackPresenter)から状態を取得し、
    /// 「前を向いている(=背後を向いていない、ライトも点いていない)」間だけ、
    /// アタッチされたオブジェクトを歩くように前進させる。
    /// </summary>
    public class PlayerForwardWalker : MonoBehaviour
    {
        [Header("差し込み口(Editor確認用。Android実機ではAndroidManagerを優先使用)")]
        [SerializeField] private MonoBehaviour inputSource;    // IPlayerInputを実装したもの
        [SerializeField] private MonoBehaviour feedbackSource; // IFeedbackPresenterを実装したもの

        [Header("歩行設定")]
        [SerializeField] private float walkSpeed = 1.2f;
        [SerializeField] private float bobHeight = 0.03f;
        [SerializeField] private float bobSpeed = 6f;

        private const float ZResetThreshold = 175.86f;

        private IPlayerInput input;
        private IFeedbackPresenter feedback;
        private float bobTimer;
        private float baseLocalY;

        private void Awake()
        {
            baseLocalY = transform.localPosition.y;
        }

        private void Start()
        {
            TryAcquireDependencies();
        }

        // AndroidManager側のAwake()がこちらより先に走っている保証がないため、
        // 取得できるまでUpdateから毎フレーム呼び直す
        private void TryAcquireDependencies()
        {
            if (input != null) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            input = AndroidManager.Instance.PlayerInput;
            feedback = AndroidManager.Instance.Feedback;
#else
            input = inputSource as IPlayerInput;
            feedback = feedbackSource as IFeedbackPresenter;
#endif
        }

        private void Update()
        {
            if (input == null) TryAcquireDependencies();
            if (input == null) return;

            // 背後を向いてライトを点けている間(=進行ゲートが閉じている間)は歩かない
            bool isFacingForward = !input.IsTurnedBack && (feedback == null || !feedback.IsLightOn);

            if (!isFacingForward)
            {
                bobTimer = 0f;
                return;
            }

            transform.Translate(Vector3.forward * (walkSpeed * Time.deltaTime), Space.Self);

            // 歩いている雰囲気を出すための上下ゆれ
            bobTimer += Time.deltaTime * bobSpeed;
            Vector3 pos = transform.localPosition;
            pos.y = baseLocalY + Mathf.Abs(Mathf.Sin(bobTimer)) * bobHeight;

            // Zが一定距離まで進んだらループさせるため0に戻す
            if (pos.z >= ZResetThreshold)
            {
                pos.z = 0f;
            }

            transform.localPosition = pos;
        }
    }
}
