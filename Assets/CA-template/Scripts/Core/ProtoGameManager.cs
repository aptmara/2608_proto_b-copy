using UnityEngine;
using TMPro;

namespace Proto.UI
{
    /// <summary>
    /// デバッグ用のUI表示および簡易操作を管理するクラス
    /// </summary>
    public class ProtoDebugUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField, Tooltip("FPSを表示するTMP_Text（任意）")]
        private TMP_Text fpsText;

        [SerializeField, Tooltip("デバッグログを表示するTMP_Text（任意）")]
        private TMP_Text logText;

        [SerializeField, Tooltip("デバッグ表示全体の親オブジェクト（任意）")]
        private GameObject debugPanel;

        private float deltaTime;

        private void Update()
        {
            UpdateFPS();
        }

        /// <summary>
        /// 画面上に簡易的なFPSを表示
        /// </summary>
        private void UpdateFPS()
        {
            if (fpsText == null) return;

            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;
            fpsText.text = $"FPS: {Mathf.CeilToInt(fps)}";
        }

        /// <summary>
        /// デバッグパネルの表示 / 非表示切り替え
        /// </summary>
        public void ToggleDebugPanel()
        {
            if (debugPanel != null)
            {
                debugPanel.SetActive(!debugPanel.activeSelf);
            }
            else
            {
                gameObject.SetActive(!gameObject.activeSelf);
            }
        }

        /// <summary>
        /// 画面上にログを表示するためのメソッド
        /// </summary>
        public void SetLogMessage(string message)
        {
            if (logText != null)
            {
                logText.text = message;
            }
        }
    }
}