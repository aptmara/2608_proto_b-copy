using UnityEngine;

namespace SoroSoro.Events
{
    /// <summary>
    /// GameOverシーン内で写真ボタンのクリックを検知し、
    /// FadeManager経由でタイトルシーンへ遷移するクラス。
    /// 写真ボタンは、このシーンに配置したButtonHandlerコンポーネントの
    /// targetButtonとして割り当ててください。
    /// </summary>
    public sealed class GameOverToTitleTransition : MonoBehaviour
    {
        [SerializeField] private string titleSceneName = "Title";

        private void Start()
        {
            if (ButtonHandler.Instance != null)
            {
                ButtonHandler.Instance.OnButtonClicked += OnPhotoButtonClicked;
            }
            else
            {
                Debug.LogError("[GameOverToTitleTransition] ButtonHandler がシーン内に存在しません。");
            }
        }

        private void OnDestroy()
        {
            if (ButtonHandler.HasInstance)
            {
                ButtonHandler.Instance.OnButtonClicked -= OnPhotoButtonClicked;
            }
        }

        private void OnPhotoButtonClicked()
        {
            if (FadeManager.Instance != null)
            {
                FadeManager.Instance.FadeToScene(titleSceneName);
            }
            else
            {
                Debug.LogError("[GameOverToTitleTransition] FadeManager がシーン内に存在しません。");
            }
        }
    }
}