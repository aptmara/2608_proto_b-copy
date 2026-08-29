using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIDialogManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text dialogText;
    [SerializeField] private CanvasGroup dialogCanvasGroup;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.3f;

    private Coroutine fadeCoroutine;
    private bool isOpen = false;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        if (dialogCanvasGroup == null)
        {
            dialogCanvasGroup = GetComponent<CanvasGroup>();
        }

        SetDialogStateImmediate(false);
    }

    /// <summary>
    /// メッセージを指定してダイアログをフェードイン表示
    /// </summary>
    public void OpenDialog(string message)
    {
        if (dialogText != null)
        {
            dialogText.text = message;
        }

        isOpen = true;
        StartFade(1f, true);
    }

    /// <summary>
    /// 引数なしでダイアログをフェードイン表示
    /// </summary>
    public void OpenDialog()
    {
        OpenDialog("ダイアログが表示されました。");
    }

    /// <summary>
    /// ダイアログをフェードアウト非表示（ButtonのOnClickから直接呼び出し）
    /// </summary>
    public void CloseDialog()
    {
        isOpen = false;
        StartFade(0f, false);
    }

    private void StartFade(float targetAlpha, bool interactable)
    {
        if (!gameObject.activeInHierarchy)
        {
            SetDialogStateImmediate(targetAlpha > 0f);
            return;
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha, interactable));
    }

    private IEnumerator FadeRoutine(float targetAlpha, bool interactable)
    {
        if (dialogCanvasGroup == null) yield break;

        float startAlpha = dialogCanvasGroup.alpha;
        float elapsedTime = 0f;

        if (interactable)
        {
            dialogCanvasGroup.blocksRaycasts = true;
            dialogCanvasGroup.interactable = true;
        }

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            dialogCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            yield return null;
        }

        dialogCanvasGroup.alpha = targetAlpha;

        if (!interactable)
        {
            dialogCanvasGroup.blocksRaycasts = false;
            dialogCanvasGroup.interactable = false;
        }

        fadeCoroutine = null;
    }

    /// <summary>
    /// アニメーションなしで即座に状態を切り替えます
    /// </summary>
    public void SetDialogStateImmediate(bool visible)
    {
        if (dialogCanvasGroup == null) return;

        isOpen = visible;
        dialogCanvasGroup.alpha = visible ? 1f : 0f;
        dialogCanvasGroup.blocksRaycasts = visible;
        dialogCanvasGroup.interactable = visible;
    }
}