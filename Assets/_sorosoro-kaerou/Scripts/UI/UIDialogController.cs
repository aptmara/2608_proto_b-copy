using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDialogController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text dialogText;
    [SerializeField] private Button closeButton;

    [Header("Dialog Target")]
    [Tooltip("表示・非表示を切り替えるオブジェクト（未設定の場合はこのGameObject自身）")]
    [SerializeField] private GameObject dialogRoot;

    private void Awake()
    {
        if (dialogRoot == null)
        {
            dialogRoot = gameObject;
        }

        // シーン内に1つあるボタンを自動取得（インスペクター未設定時）
        if (closeButton == null)
        {
            closeButton = FindFirstObjectByType<Button>();
        }

        // 初期状態は非表示
        CloseDialog();
    }

    private void OnEnable()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseDialog);
        }
    }

    private void OnDisable()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseDialog);
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