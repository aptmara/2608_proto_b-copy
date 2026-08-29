using System.Collections;
using UnityEngine;

public class TitleSceneChanger : MonoBehaviour
{
    [Header("UI Component References")]
    [SerializeField] private CanvasGroup titleCanvasGroup;
    [SerializeField] private UIDialogController dialogController;

    [Header("Transition Settings")]
    [SerializeField] private string nextSceneName = "MainGame";
    [SerializeField] private float fadeDuration = 1.0f;
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
        // OnEnableではなくStartでInstanceを呼び出し、確実に登録する
        if (ButtonHandler.Instance != null)
        {
            ButtonHandler.Instance.OnButtonClicked += OnButtonClicked;
        }
    }

    private void OnDestroy()
    {
        if (ButtonHandler.HasInstance)
        {
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

        if (dialogController != null)
        {
            dialogController.OpenDialog(dialogMessage);
        }

        currentState = State.WaitingForDialogClick;
    }
}