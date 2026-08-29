using UnityEngine;

public class TestScriptsNoguchi : MonoBehaviour
{
    [SerializeField] private UIDialogController dialogController;

    private void Start()
    {
        if (dialogController == null)
        {
            dialogController = FindFirstObjectByType<UIDialogController>();
        }
    }

    private void Update()
    {
        // Spaceキーでシーン移動
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FadeManager.Instance.FadeToScene("Title");
        }

        // Tキーでダイアログ表示（デバッグ用）
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (dialogController != null)
            {
                dialogController.OpenDialog("Tキーが押されました！\nボタンを押すと閉じます。");
            }
            else
            {
                Debug.LogWarning("[TestScriptsNoguchi] UIDialogController が見つかりません。");
            }
        }
    }
}