using UnityEngine;
using UnityEngine.UI;

public class UIButtonController : MonoBehaviour
{
    [SerializeField] private Button targetButton;

    private void OnEnable()
    {
        if (targetButton != null)
        {
            // ボタン押下時に関数を実行するよう登録
            targetButton.onClick.AddListener(OnButtonClicked);
        }
    }

    private void OnDisable()
    {
        if (targetButton != null)
        {
            // メモリリーク防止のため登録解除
            targetButton.onClick.RemoveListener(OnButtonClicked);
        }
    }

    /// <summary>
    /// ボタンが押された時に実行される処理
    /// </summary>
    private void OnButtonClicked()
    {
        Debug.Log("ボタンが押されました！");
    }
}