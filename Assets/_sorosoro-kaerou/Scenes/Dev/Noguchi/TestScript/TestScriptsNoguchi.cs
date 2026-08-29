using UnityEngine;

public class TestScriptsNoguchi : MonoBehaviour
{
    [SerializeField] private UIDialogManager dialogController;

    private void Start()
    {
        if (dialogController == null)
        {
            dialogController = FindFirstObjectByType<UIDialogManager>(FindObjectsInactive.Include);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (FadeManager.Instance != null)
            {
                FadeManager.Instance.FadeToScene("Title");
            }
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (dialogController != null)
            {
                dialogController.OpenDialog("Tキーが押されました！\nボタンを押すと閉じます。");
            }
            else
            {
                Debug.LogWarning("[TestScriptsNoguchi] UIDialogManager が見つかりません。Inspectorで割当を確認してください。");
            }
        }
    }
}