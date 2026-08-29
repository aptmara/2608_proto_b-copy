using UnityEngine;

namespace SoroSoro.Events
{
    /// <summary>
    /// OnGameOver イベントを受け取り、FadeManager 経由で GameOver シーンへ遷移するクラス。
    /// </summary>
    public sealed class GameOverSceneTransition : MonoBehaviour
    {
        [SerializeField] private string gameOverSceneName = "GameOver";

        private void OnEnable()
        {
            GameEvents.OnGameOver += HandleGameOver;
        }

        private void OnDisable()
        {
            GameEvents.OnGameOver -= HandleGameOver;
        }

        private void HandleGameOver(ResultData data)
        {
            if (FadeManager.Instance != null)
            {
                FadeManager.Instance.FadeToScene(gameOverSceneName);
            }
            else
            {
                Debug.LogError("[GameOverSceneTransition] FadeManager がシーン内に存在しません。");
            }
        }
    }
}