using UnityEngine;
using UnityEngine.UI;

namespace SoroSoro.Events
{
    /// <summary>
    /// GameClearResultを別シーンではなく同一シーン内のCanvasとして出し入れするコントローラ。
    ///
    /// Additiveロード/アンロードやButtonHandler(Singleton)を経由しないため、
    /// シーン名の一致・Singletonの生存状態に依存しない。時間が無いときの安全策。
    ///
    /// 全ての参照をInspectorで直接刺す。FindFirstObjectByTypeは使わない。
    /// </summary>
    public sealed class GameClearPanelController : MonoBehaviour
    {
        [Header("結線")]
        [SerializeField, Tooltip("GameClearResultのCanvasオブジェクト本体")]
        private GameObject panelRoot;

        [SerializeField, Tooltip("同じオブジェクトに付いているGameClearResultHandler")]
        private GameClearResultHandler handler;

        [SerializeField, Tooltip("次の日へ進む撮影ボタン")]
        private Button continueButton;

        private void OnEnable()
        {
            GameEvents.OnDayCleared += HandleDayCleared;

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
            }
            else
            {
                Debug.LogError("[GameClearPanelController] continueButtonが未設定です。", this);
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void OnDisable()
        {
            GameEvents.OnDayCleared -= HandleDayCleared;

            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(OnContinueClicked);
            }
        }

        private void HandleDayCleared(ResultData data)
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
            else
            {
                Debug.LogError("[GameClearPanelController] panelRootが未設定です。", this);
            }

            if (handler != null)
            {
                handler.DisplayDayResult(data);
            }
            else
            {
                Debug.LogError("[GameClearPanelController] handlerが未設定です。", this);
            }
        }

        private void OnContinueClicked()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            GameEvents.RaiseDayClearContinue();
        }
    }
}
