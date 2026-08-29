using TMPro;
using UnityEngine;

public class UIDialogController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text dialogText;

    [Header("Dialog Target")]
    [Tooltip("表示・非表示を切り替えるオブジェクト（未設定の場合はこのGameObject自身）")]
     private GameObject dialogRoot;

    private void Awake()
    {
        dialogRoot = gameObject;

        // 初期状態は非表示
        CloseDialog();
    }

    private void OnEnable()
    {
        // ButtonHandlerのクリックイベントを購読
        if (ButtonHandler.HasInstance)
        {
            ButtonHandler.Instance.OnButtonClicked += OnTargetButtonClicked;
        }
    }

    private void OnDisable()
    {
        // イベント解除
        if (ButtonHandler.HasInstance)
        {
            ButtonHandler.Instance.OnButtonClicked -= OnTargetButtonClicked;
        }
    }

    /// <summary>
    /// ButtonHandler経由でボタンが押された時の処理
    /// </summary>
    private void OnTargetButtonClicked()
    {
        // ボタンが押された時の動作（例: ダイアログを開く / 閉じる）
        // 必要に応じて処理を変更してください
        if (dialogRoot != null && dialogRoot.activeSelf)
        {
            CloseDialog();
        }
        else
        {
            OpenDialog();
        }
    }

    /// <summary>
    /// UnityEvent用：メッセージを指定してダイアログを開く
    /// </summary>
    public void OpenDialog(string message)
    {
        if (dialogText != null)
        {
            dialogText.text = message;
        }

        if (dialogRoot != null)
        {
            dialogRoot.SetActive(true);
        }
    }

    /// <summary>
    /// UnityEvent用：引数なしでダイアログを開く
    /// </summary>
    public void OpenDialog()
    {
        OpenDialog("ダイアログが表示されました。");
    }

    /// <summary>
    /// UnityEvent用：ダイアログを閉じる
    /// </summary>
    public void CloseDialog()
    {
        if (dialogRoot != null)
        {
            dialogRoot.SetActive(false);
        }
    }
}