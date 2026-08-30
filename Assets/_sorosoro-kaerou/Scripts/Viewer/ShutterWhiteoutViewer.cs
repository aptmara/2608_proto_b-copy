using System.Collections;
using UnityEngine;

namespace SoroSoro.Events
{
    /// <summary>
    /// 撮影のInstantiateと同時（OnShutterRequested）に画面を白くフェードインし、
    /// 少し保持してからフェードアウトする演出。
    /// 白で覆っている間に GhostApproachViewer 等が OnPhotoCaptured を受けて
    /// 怪異画像へ差し替えるため、切り替えの瞬間を自然に隠す役割も兼ねる。
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ShutterWhiteoutViewer : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("タイミング（秒）")]
        [Tooltip("白へフェードインするまでの時間。短いほど「パッ」と光る")]
        [SerializeField] private float fadeInDuration = 0.05f;

        [Tooltip("真っ白を保持する時間。この間に判定確定〜怪異画像の差し替えが完了する")]
        [SerializeField] private float holdDuration = 0.2f;

        [Tooltip("白から元の画面へ戻るまでの時間")]
        [SerializeField] private float fadeOutDuration = 0.25f;

        private Coroutine routine;

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            SetAlphaImmediate(0f);
        }

        private void OnEnable()
        {
            GameEvents.OnShutterRequested += HandleShutterRequested;
        }

        private void OnDisable()
        {
            GameEvents.OnShutterRequested -= HandleShutterRequested;
        }

        private void HandleShutterRequested()
        {
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(WhiteoutRoutine());
        }

        private IEnumerator WhiteoutRoutine()
        {
            if (canvasGroup != null) canvasGroup.blocksRaycasts = true;

            yield return Fade(1f, fadeInDuration);
            yield return new WaitForSeconds(holdDuration);
            yield return Fade(0f, fadeOutDuration);

            if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
            routine = null;
        }

        private IEnumerator Fade(float target, float duration)
        {
            if (canvasGroup == null) yield break;

            float start = canvasGroup.alpha;
            if (duration <= 0f)
            {
                canvasGroup.alpha = target;
                yield break;
            }

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(start, target, t / duration);
                yield return null;
            }
            canvasGroup.alpha = target;
        }

        private void SetAlphaImmediate(float alpha)
        {
            if (canvasGroup == null) return;
            canvasGroup.alpha = alpha;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }
}
