using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SoroSoro.Events
{
    /// <summary>
    /// OnDayCleared イベントを受け取り、GameClearResult シーンを Additive で読み込み、
    /// ButtonHandler のボタン検知を経て DayClearContinue を発行するクラス。
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
            UnsubscribeButton();
        }

        private void HandleDayCleared(ResultData data)
        {
            StartCoroutine(ShowClearResultRoutine(data));
        }

        private IEnumerator ShowClearResultRoutine(ResultData data)
        {
            yield return SceneManager.LoadSceneAsync(clearResultSceneName, LoadSceneMode.Additive);

            // 読み込んだ GameClearResult シーン内のハンドラにリザルトを渡す
            var handler = FindFirstObjectByType<GameClearResultHandler>();
            if (handler != null)
            {
                handler.DisplayDayResult(data);
            }
            else
            {
                Debug.LogError("[GameClearSceneTransition] GameClearResultHandler がシーン内に存在しません。");
            }

            if (ButtonHandler.Instance != null)
            {
                ButtonHandler.Instance.OnButtonClicked += OnButtonClicked;
            }
            else
            {
                Debug.LogError("[GameClearSceneTransition] ButtonHandler がシーン内に存在しません。");
            }
        }

        private void OnButtonClicked()
        {
            UnsubscribeButton();
            GameEvents.RaiseDayClearContinue();
        }

        private void UnsubscribeButton()
        {
            if (ButtonHandler.HasInstance)
            {
                ButtonHandler.Instance.OnButtonClicked -= OnButtonClicked;
            }
        }
    }
}