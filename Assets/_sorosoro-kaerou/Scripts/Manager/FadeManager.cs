using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class FadeManager : Singleton<FadeManager>
{
    [SerializeField] private GameObject loadingCanvas;
    
    [SerializeField, Header("フェードにかかる時間（秒）")]
    private float fadeDuration = 1.0f;

    [SerializeField, Header("フェードインからフェードアウトまでの待機時間（秒）")]
    private float holdDuration = 0.5f;
    
    private CanvasGroup canvasGroup;
    private bool isChanged = false;

    /// <summary> 直前のシーン名を取得します </summary>
    public string PreviousSceneName { get; private set; } = string.Empty;

    /// <summary> 現在フェード処理中かどうかを取得します </summary>
    public bool IsFading => isChanged;

    protected override void Awake()
    {
        base.Awake();
        canvasGroup = GetComponent<CanvasGroup>();
        
        // 初期状態では透明・クリック判定なしにしておく
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
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

        // フェードイン（画面を暗くする：Alpha 0 -> 1）
        yield return StartCoroutine(Fade(1f));

        // シーンロード直前に Loading 表示をオン
        // （※ loadingCanvas が FadeManager 自身（gameObject）を指していると
        //   非アクティブ化でコルーチンが停止しフェードが完走しないため、自オブジェクトは対象外にする）
        if (loadingCanvas != null && loadingCanvas != gameObject)
        {
            loadingCanvas.SetActive(true);
        }

        // シーンをロード（軽量なため同期ロード）
        SceneManager.LoadScene(sceneName);

        // フェードアウト開始直前に Loading 表示をオフ
        if (loadingCanvas != null && loadingCanvas != gameObject)
        {
            loadingCanvas.SetActive(false);
        }

        // フェードインとフェードアウトの間の待機（画面を真っ黒のまま保持）
        yield return new WaitForSecondsRealtime(holdDuration);

        // フェードアウト（画面を明るくする：Alpha 1 -> 0）
        yield return StartCoroutine(Fade(0f));

        // UI操作を有効化
        SetNavigationEvents(true);

        isChanged = false;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (canvasGroup == null) yield break;

        // フェード中は念のためCanvasGroupのクリックブロックを有効化
        canvasGroup.blocksRaycasts = true;

        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        // 指定した時間（fadeDuration）かけてAlpha値を変化させる
        while (time < fadeDuration)
        {
            // Time.unscaledDeltaTimeを使用し、TimeScaleの変更に影響されないようにする
            time += Time.unscaledDeltaTime;
            
            // 0～1の割合を計算し、Alphaを補間
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            
            yield return null;
        }

        // 最終的なAlpha値を確実にセット
        canvasGroup.alpha = targetAlpha;

        // フェードアウト（画面が完全に見える状態）完了時のみクリックブロックを解除
        if (targetAlpha == 0f)
        {
            canvasGroup.blocksRaycasts = false;
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