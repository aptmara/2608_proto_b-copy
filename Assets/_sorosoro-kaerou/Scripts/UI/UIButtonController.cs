using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIButtonController : MonoBehaviour
{
    [SerializeField] private Button targetButton;
    
    [Header("Button Event")]
    [SerializeField] private UnityEvent onButtonClicked;

    private void Awake()
    {
        if (targetButton == null)
        {
            targetButton = GetComponent<Button>() ?? FindFirstObjectByType<Button>();
        }
    }

    private void OnEnable()
    {
        if (targetButton != null)
        {
            targetButton.onClick.AddListener(OnButtonClicked);
        }
    }

    private void OnDisable()
    {
        if (targetButton != null)
        {
            targetButton.onClick.RemoveListener(OnButtonClicked);
        }
    }

    private void OnButtonClicked()
    {
        Debug.Log("ボタンが押されました！");
        onButtonClicked?.Invoke();
    }
}