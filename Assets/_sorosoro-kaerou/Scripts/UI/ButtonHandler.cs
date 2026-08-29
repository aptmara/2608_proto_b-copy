using System;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : Singleton<ButtonHandler>
{
    [SerializeField] private Button targetButton;

    public event Action OnButtonClicked;

    protected override void Awake()
    {
        if (HasInstance && Instance != this)
        {
            Instance.SetTargetButton(targetButton);
            Destroy(gameObject);
            return;
        }

        base.Awake();
        SetTargetButton(targetButton);
    }

    public void SetTargetButton(Button newButton)
    {
        if (targetButton != null)
        {
            targetButton.onClick.RemoveListener(HandleButtonClick);
        }

        targetButton = newButton;

        if (targetButton != null)
        {
            targetButton.onClick.AddListener(HandleButtonClick);
        }
    }

    private void HandleButtonClick()
    {
        OnButtonClicked?.Invoke();
    }

    protected override void OnDestroy()
    {
        if (targetButton != null)
        {
            targetButton.onClick.RemoveListener(HandleButtonClick);
        }

        base.OnDestroy();
    }
}