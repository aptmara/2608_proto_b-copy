using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SoroSoro.Events
{
    /// <summary>
    /// OnDayCleared イベントを受け取り、GameClearResult シーンを Additive で読み込み、
    /// ButtonHandler のボタン検知を経て DayClearContinue を発行するクラス。
    ///
    /// GameClearResultシーンのロード・アンロードはこのクラスだけが行います。
    /// GameManager側では一切シーン操作をしません（二重ロードになるため）。
    /// </summary>
    public sealed class GameClearSceneTransition : MonoBehaviour
    {
        [SerializeField] private string clearResultSceneName = "GameClearResult";

        // 表示中の再入を防ぐ。ロード中に再度OnDayClearedが来てもシーンが積み上がらない。
        private bool isShowing;

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
            if (isShowing) return;
            isShowing = true;
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
            StartCoroutine(CloseClearResultRoutine());
        }

        /// <summary>
        /// アンロードの完了を待ってから DayClearContinue を発行します。
        /// 順序を逆にすると、リザルトがまだ画面に出ている状態で次の日の音イベントが動き出します。
        /// </summary>
        private IEnumerator CloseClearResultRoutine()
        {
            var scene = SceneManager.GetSceneByName(clearResultSceneName);
            if (scene.isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(scene);
            }

            isShowing = false;
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