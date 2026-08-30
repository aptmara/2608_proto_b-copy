using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TutorialSkipButton : MonoBehaviour
{
    [SerializeField, Header("遷移先のシーン名")]
    private string targetSceneName = "MainGame";

    private Button skipButton;

    private void Awake()
    {
        // 同じオブジェクトにアタッチされているButtonコンポーネントを取得
        skipButton = GetComponent<Button>();
    }

    private void OnEnable()
    {
        // ボタンのクリックイベントにメソッドを登録
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(OnSkipButtonClicked);
        }
    }

    private void OnDisable()
    {
        // 破棄・非アクティブ化時にイベントの登録を解除（メモリリーク防止）
        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(OnSkipButtonClicked);
        }
    }

    private void OnSkipButtonClicked()
    {
        // FadeManager のシングルトンインスタンスを通じてシーン遷移を実行
        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.FadeToScene(targetSceneName);
        }
        else
        {
            Debug.LogError("[TutorialSkipButton] FadeManagerのインスタンスが見つかりません。");
        }
    }
}