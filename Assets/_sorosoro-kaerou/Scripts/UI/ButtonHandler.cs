using System;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : Singleton<ButtonHandler>
{
    [SerializeField] private Button targetButton;

    /// <summary>
    /// ボタンがクリックされた時に発火するイベント
    /// </summary>
    public event Action OnButtonClicked;

    protected override void Awake()
    {
        // 既にインスタンスが存在する場合（シーン遷移で残った既存インスタンスがある場合）
        if (HasInstance && Instance != this)
        {
            // 新シーンのButton参照を既存のSingletonインスタンスに引き継ぐ
            Instance.SetTargetButton(targetButton);

            // 重複した自身は破棄
            Destroy(gameObject);
            return;
        }

        // 最初のインスタンス生成時
        base.Awake();
        SetTargetButton(targetButton);
    }

    /// <summary>
    /// 監視対象のボタンを付け替え、クリックイベントを設定します
    /// </summary>
    public void SetTargetButton(Button newButton)
    {
        // 旧ボタンのリスナー解除
        if (targetButton != null)
        {
            targetButton.onClick.RemoveListener(HandleButtonClick);
        }

        targetButton = newButton;

        // 新ボタンへのリスナー登録
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