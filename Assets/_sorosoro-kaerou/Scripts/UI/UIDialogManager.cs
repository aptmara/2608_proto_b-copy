using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events; // ★追加

[RequireComponent(typeof(CanvasGroup))]
public class UIDialogManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text dialogText;
    [SerializeField] private CanvasGroup dialogCanvasGroup;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    
    [Header("Events")] // ★追加
    public UnityEvent OnClosedComplete; // ★追加: 閉じ終わった時のイベント

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

    public void OpenDialog(string message)
    {
        if (dialogText != null) dialogText.text = message;
        isOpen = true;
        StartFade(1f, true);
    }

    public void OpenDialog() => OpenDialog("ダイアログが表示されました。");

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

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
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
            
            // ★追加: フェードアウトが完全に終わったらイベントを発行
            OnClosedComplete?.Invoke(); 
        }

        fadeCoroutine = null;
    }

    public void SetDialogStateImmediate(bool visible)
    {
        if (dialogCanvasGroup == null) return;
        isOpen = visible;
        dialogCanvasGroup.alpha = visible ? 1f : 0f;
        dialogCanvasGroup.blocksRaycasts = visible;
        dialogCanvasGroup.interactable = visible;
    }
}