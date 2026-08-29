using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class FadeManager : Singleton<FadeManager>
{
    [SerializeField] private GameObject loadingCanvas;
    
    private Animator anim;
    private bool isChanged = false;

    /// <summary> 直前のシーン名を取得します </summary>
    public string PreviousSceneName { get; private set; } = string.Empty;

    /// <summary> 現在フェード処理中かどうかを取得します </summary>
    public bool IsFading => isChanged;

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<Animator>();
    }

    /// <summary>
    /// 指定したシーンへフェード付きで移動します
    /// </summary>
    /// <param name="sceneName">移動先のシーン名</param>
    /// <returns>処理が開始された場合は true</returns>
    public bool FadeToScene(string sceneName)
    {
        if (isChanged)
        {
            Debug.LogWarning("[FadeManager] すでにフェード処理が実行中です。");
            return false;
        }

        // 遷移前に現在のシーン名を保存
        PreviousSceneName = SceneManager.GetActiveScene().name;

        // ポーズ解除
        Time.timeScale = 1f;

        StartCoroutine(FadeAndLoad(sceneName));
        return true;
    }

    /// <summary>
    /// UnityEvent（Button等）から呼び出すためのラッパーメソッド
    /// </summary>
    public void FadeToSceneEvent(string sceneName)
    {
        FadeToScene(sceneName);
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        isChanged = true;

        // UI操作を無効化
        SetNavigationEvents(false);

        // フェードイン（画面を暗くする）
        yield return StartCoroutine(Fade(1));

        // シーンロード直前に Loading 表示をオン
        if (loadingCanvas != null)
        {
            loadingCanvas.SetActive(true);
        }

        // 非同期でシーンをロード
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.1f);

        // フェードアウト開始直前に Loading 表示をオフ
        if (loadingCanvas != null)
        {
            loadingCanvas.SetActive(false);
        }

        // フェードアウト（画面を明るくする）
        yield return StartCoroutine(Fade(0));

        // UI操作を有効化
        SetNavigationEvents(true);

        isChanged = false;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (anim == null) yield break;

        string stateName = targetAlpha == 1 ? "FadeIn" : "FadeOut";
        anim.SetTrigger(stateName);

        yield return null; // 1フレーム待機してAnimatorのステート更新を反映

        // 指定ステートに遷移完了するまで待機
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            yield return null;
        }

        // アニメーションの再生が完了するまで待機
        while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }
    }

    private void SetNavigationEvents(bool enabled)
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.sendNavigationEvents = enabled;
        }
    }
}