using System.Collections;
using UnityEngine;

namespace SoroSoro.Events
{
    /// <summary>
    /// ProgressChangedイベントを受け取り、UIのY座標を指定の範囲(-350 〜 350)で移動させるコンポーネントです。
    /// WastedViewerに倣い、コルーチンを使って滑らかに位置を補間します。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class ProgressBarViewer : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private RectTransform rectTransform;

        [Header("Position Settings")]
        [SerializeField] private float minY = -350f;
        [SerializeField] private float maxY = 350f;

        [Header("Animation Settings")]
        [SerializeField] private float moveDuration = 0.5f;

        private Coroutine currentCoroutine;

        private void Awake()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }
        }

        private void OnEnable()
        {
            // GameEvents からの進行度変更イベントを購読する
            GameEvents.OnProgressChanged += HandleProgressChanged;
        }

        private void OnDisable()
        {
            // 購読解除（メモリリーク防止）
            GameEvents.OnProgressChanged -= HandleProgressChanged;
        }

        /// <summary>
        /// GameEvents.OnProgressChanged から呼び出されるハンドラー
        /// </summary>
        /// <param name="progressValue">0.0 から 1.0 の進行度を想定</param>
        private void HandleProgressChanged(float progressValue)
        {
            // 進行度を安全のため 0.0 〜 1.0 の範囲に収める
            progressValue = Mathf.Clamp01(progressValue);
            
            // 進行度に応じた目標のY座標を計算（0のとき-350、1のとき350）
            float targetY = Mathf.Lerp(minY, maxY, progressValue);

            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }
            
            // moveDuration が 0 より大きければアニメーション、それ以外は即時反映
            if (moveDuration > 0f)
            {
                currentCoroutine = StartCoroutine(MoveYRoutine(targetY));
            }
            else
            {
                SetPositionYImmediate(targetY);
            }
        }

        private IEnumerator MoveYRoutine(float targetY)
        {
            if (rectTransform == null) yield break;

            float startY = rectTransform.anchoredPosition.y;
            float elapsedTime = 0f;

            while (elapsedTime < moveDuration)
            {
                elapsedTime += Time.deltaTime;
                
                // 開始位置から目標位置へなめらかに補間
                float currentY = Mathf.Lerp(startY, targetY, elapsedTime / moveDuration);
                SetPositionYImmediate(currentY);

                yield return null;
            }

            // 最終的にズレがないようピッタリ合わせる
            SetPositionYImmediate(targetY);
            currentCoroutine = null;
        }

        /// <summary>
        /// Y座標を即座に更新する
        /// </summary>
        private void SetPositionYImmediate(float y)
        {
            if (rectTransform == null) return;
            
            Vector2 pos = rectTransform.anchoredPosition;
            pos.y = y;
            rectTransform.anchoredPosition = pos;
        }
    }
}