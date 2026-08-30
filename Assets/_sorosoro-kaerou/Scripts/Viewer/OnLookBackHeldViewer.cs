using System.Collections;
using UnityEngine;

namespace SoroSoro.Events
{
    /// <summary>
    /// 後ろを一定時間以上向き続けた際 (GameEvents.OnLookBackHeld) にトリガーされ、
    /// CanvasGroup を使ってフェードイン表示したあと、一定時間後に自動でフェードアウトさせるコンポーネントです。
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class OnLookBackHeldViewer : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private float displayDuration = 2.0f; // 表示し続ける時間

        private Coroutine currentCoroutine;

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            // 初期状態は完全に非表示
            SetStateImmediate(false);
        }

        private void OnEnable()
        {
            // GameEvents からの OnLookBackHeld イベントを購読する
            GameEvents.OnLookBackHeld += HandleLookBackHeld;
        }

        private void OnDisable()
        {
            // 購読解除（メモリリーク防止）
            GameEvents.OnLookBackHeld -= HandleLookBackHeld;
        }

        /// <summary>
        /// GameEvents.OnLookBackHeld から呼び出されるハンドラー
        /// </summary>
        private void HandleLookBackHeld()
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }
            currentCoroutine = StartCoroutine(LookBackHeldSequenceRoutine());
        }

        private IEnumerator LookBackHeldSequenceRoutine()
        {
            // 1. フェードイン
            yield return StartCoroutine(FadeRoutine(1f, true));

            // 2. 指定時間待機
            yield return new WaitForSeconds(displayDuration);

            // 3. フェードアウト
            yield return StartCoroutine(FadeRoutine(0f, false));

            currentCoroutine = null;
        }

        private IEnumerator FadeRoutine(float targetAlpha, bool interactable)
        {
            if (canvasGroup == null) yield break;

            float startAlpha = canvasGroup.alpha;
            float elapsedTime = 0f;

            if (interactable)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;

            if (!interactable)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }

        private void SetStateImmediate(bool visible)
        {
            if (canvasGroup == null) return;
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.blocksRaycasts = visible;
            canvasGroup.interactable = visible;
        }
    }
}