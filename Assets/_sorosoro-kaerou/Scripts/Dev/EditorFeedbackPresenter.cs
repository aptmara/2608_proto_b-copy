using System.Collections;
using SoroSoro.Events;
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

        private bool isLightOn;

        public void SetLight(bool on)
        {
            if (overlayImage == null) return;
            Color color = overlayImage.color;
            color.a = on ? lightAlpha : 0f;
            overlayImage.color = color;

            isLightOn = on;
            GameEvents.RaiseLightOnChanged(on);
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
            GameEvents.RaiseLightOnChanged(true);

            yield return new WaitForSeconds(flashDuration);

            overlayImage.color = originalColor;
            // フラッシュ終了後は、振り返り中ならON、そうでなければOFFの状態に戻す
            GameEvents.RaiseLightOnChanged(isLightOn);
        }
    }
}