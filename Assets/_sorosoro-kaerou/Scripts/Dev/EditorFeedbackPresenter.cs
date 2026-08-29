using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;

namespace SorosoroKaerou
{
    /// <summary>
    /// Unity Editor確認用の画面エフェクト・ログ出力フィードバック実装
    /// </summary>
    public sealed class EditorFeedbackPresenter : MonoBehaviour, IFeedbackPresenter
    {
        public bool IsLightOn => false;

        [SerializeField] private Image overlayImage;
        [SerializeField] private float lightAlpha = 0.3f;
        [SerializeField] private float flashDuration = 0.1f;

        public void SetLight(bool on)
        {
            if (overlayImage == null) return;
            Color color = overlayImage.color;
            color.a = on ? lightAlpha : 0f;
            overlayImage.color = color;
        }

        public void Flash()
        {
            StartCoroutine(FlashRoutine());
        }

        public void Vibrate()
        {
            Debug.Log("[EditorFeedbackPresenter] Vibrate called");
        }

        private IEnumerator FlashRoutine()
        {
            if (overlayImage == null) yield break;

            Color originalColor = overlayImage.color;
            overlayImage.color = Color.white;

            yield return new WaitForSeconds(flashDuration);

            overlayImage.color = originalColor;
        }
    }
}