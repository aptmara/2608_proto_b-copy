using UnityEngine;

namespace SoroSoro.Events
{
    /// <summary>
    /// OnDayCleared イベントを受け取り、FadeManager 経由で GameClearResult シーンへ遷移するクラス。
    /// </summary>
    public sealed class GameClearSceneTransition : MonoBehaviour
    {
        [SerializeField] private string clearResultSceneName = "GameClearResult";

        private void OnEnable()
        {
            GameEvents.OnDayCleared += HandleDayCleared;
        }

        private void OnDisable()
        {
            GameEvents.OnDayCleared -= HandleDayCleared;
        }

        private void HandleDayCleared(int day)
        {
            if (FadeManager.Instance != null)
            {
                FadeManager.Instance.FadeToScene(clearResultSceneName);
            }
            else
            {
                Debug.LogError("[GameClearSceneTransition] FadeManager がシーン内に存在しません。");
            }
        }
    }
}