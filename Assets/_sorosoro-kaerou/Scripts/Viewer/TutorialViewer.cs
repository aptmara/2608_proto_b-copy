using UnityEngine;

public class TutorialViewer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIDialogManager dialogManager;
    
    [Header("Tutorial Data")]
    [SerializeField] private string[] tutorialTexts = {
        "テキスト1",
        "テキスト2",
        "テキスト3",
        "テキスト4",
        "テキスト5"
    };

    private int currentIndex = 0;

    private void Start()
    {
        if (dialogManager != null)
        {
            // ダイアログが「完全に閉じ終わった」時のイベントを監視する
            dialogManager.OnClosedComplete.AddListener(HandleDialogClosed);
        }

        if (tutorialTexts.Length > 0)
        {
            ShowCurrentStep();
        }
    }

    private void OnDestroy()
    {
        // メモリリーク防止のため、オブジェクト破棄時にイベント登録を解除
        if (dialogManager != null)
        {
            dialogManager.OnClosedComplete.RemoveListener(HandleDialogClosed);
        }
    }

    /// <summary>
    /// ダイアログのフェードアウト完了時に自動で呼ばれる
    /// </summary>
    private void HandleDialogClosed()
    {
        currentIndex++;

        if (currentIndex < tutorialTexts.Length)
        {
            // まだテキストが残っていれば、次のテキストを入れて再び開く
            ShowCurrentStep();
        }
        else
        {
            // 全て完了した時の処理（必要であれば）
            Debug.Log("チュートリアル完了！");
        }
    }

    private void ShowCurrentStep()
    {
        if (dialogManager != null)
        {
            dialogManager.OpenDialog(tutorialTexts[currentIndex]);
        }
    }
}