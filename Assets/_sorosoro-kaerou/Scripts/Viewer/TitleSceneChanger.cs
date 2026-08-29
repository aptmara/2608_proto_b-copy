using System.Collections;
using UnityEngine;

public class TitleSceneChanger : MonoBehaviour
{
    [Header("UI Component References")]
    [SerializeField] private CanvasGroup titleCanvasGroup;
    [SerializeField] private UIDialogManager dialogManager;

    [Header("Transition Settings")]
    [SerializeField] private string nextSceneName = "MainGame";
    [SerializeField] private float fadeDuration = 1.0f;
    
    [Tooltip("タイトルが消え切ってからダイアログが表示されるまでの待ち時間（秒）")]
    [SerializeField] private float waitBeforeDialog = 0.5f;
    
    [SerializeField] private string dialogMessage = "そろそろ帰ろう...";

    private enum State
    {
        WaitingForTitleClick,
        FadingTitleCanvas,
        WaitingForDialogClick,
        ChangingScene
    }

    private State currentState = State.WaitingForTitleClick;

    private void Start()
    {
        if (ButtonHandler.Instance != null)
        {
            ButtonHandler.Instance.OnButtonClicked += OnButtonClicked;
        }
    }

    private void OnDestroy()
    {
        if (ButtonHandler.HasInstance)
        {
            // -= OnDestroy から -= OnButtonClicked に修正
            ButtonHandler.Instance.OnButtonClicked -= OnButtonClicked;
        }
    }

    private void OnButtonClicked()
    {
        switch (currentState)
        {
            case State.WaitingForTitleClick:
                StartCoroutine(FadeOutTitleAndShowDialog());
                break;

            case State.WaitingForDialogClick:
                currentState = State.ChangingScene;

                if (dialogManager != null)
                {
                    dialogManager.CloseDialog();
                }

                if (FadeManager.Instance != null)
                {
                    FadeManager.Instance.FadeToScene(nextSceneName);
                }
                break;
        }
    }

    private IEnumerator FadeOutTitleAndShowDialog()
    {
        currentState = State.FadingTitleCanvas;

        // 1. タイトルUIをフェードアウト
        if (titleCanvasGroup != null)
        {
            float startAlpha = titleCanvasGroup.alpha;
            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                titleCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeDuration);
                yield return null;
            }

            titleCanvasGroup.alpha = 0f;
            titleCanvasGroup.blocksRaycasts = false;
            titleCanvasGroup.interactable = false;
        }

        // 2. 消え切ってからの間（ウエイト時間）を確保
        if (waitBeforeDialog > 0f)
        {
            yield return new WaitForSeconds(waitBeforeDialog);
        }

        // 3. ダイアログを表示
        if (dialogManager != null)
        {
            dialogManager.OpenDialog(dialogMessage);
        }

        currentState = State.WaitingForDialogClick;
    }
}