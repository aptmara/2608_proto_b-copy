// TutorialManager.cs
// チュートリアル専用。First/Secondそれぞれのメッセージを順に表示し、各フェーズで
// TutorialSequencerの音を鳴らして操作を確認する。
//   First  = 環境音を鳴らす → 振り返らずまっすぐ歩く
//   Second = 異常音を鳴らす → 振り返ってシャッターを撮る
// 全フェーズ完了後に FadeManager で MainGame へ遷移する。
// 入力は IPlayerInput（Editor= inputSource、実機= AndroidManager）。
using System.Collections.Generic;
using UnityEngine;
using SorosoroKaerou;

public sealed class TutorialManager : MonoBehaviour
{
    [Header("差し込み口")]
    [SerializeField] UIDialogManager dialogManager;
    [SerializeField] AudioSource audioSource;
    [SerializeField] MonoBehaviour inputSource;
    [SerializeField] SoundEventDefinition[] sequence; // 先頭=環境音、次=異常音

    [Header("文章（それぞれ複数回表示）")]
    [Tooltip("環境音の前に順番に表示するメッセージ")]
    [SerializeField] List<string> firstMessages = new List<string>();
    [Tooltip("異常音の前に順番に表示するメッセージ")]
    [SerializeField] List<string> secondMessages = new List<string>();

    [Header("遷移")]
    [SerializeField] string nextSceneName = "MainGame";

    [Header("判定")]
    [Tooltip("環境音のあと、振り返らず前方を向き続ける必要がある時間（秒）")]
    [SerializeField] float forwardConfirmSeconds = 1.5f;

    TutorialSequencer sequencer;
    IPlayerInput input;
    Step step;
    int messageIndex;
    float forwardTimer;
    SoundEventDefinition currentEvent;

    enum Step
    {
        FirstText,
        Ambient,
        SecondText,
        Anomaly,
        Transition
    }

    void Start()
    {
        sequencer = new TutorialSequencer(sequence);

        if (dialogManager == null)
        {
            dialogManager = FindFirstObjectByType<UIDialogManager>();
            if (dialogManager == null)
            {
                Debug.LogWarning("[TutorialManager] UIDialogManager が見つかりません。文章表示をスキップします。", this);
            }
        }

        if (dialogManager != null)
        {
            dialogManager.OnClosedComplete.AddListener(HandleDialogClosed);
        }

        step = Step.FirstText;
        messageIndex = 0;
        ShowFirstMessage();
    }

    void OnDestroy()
    {
        if (dialogManager != null)
        {
            dialogManager.OnClosedComplete.RemoveListener(HandleDialogClosed);
        }
    }

    void Update()
    {
        TryAcquireInput();

        if (step == Step.Ambient) UpdateAmbient();
        else if (step == Step.Anomaly) UpdateAnomaly();
    }

    void TryAcquireInput()
    {
        if (input != null) return;

#if UNITY_ANDROID && !UNITY_EDITOR
        input = AndroidManager.Instance.PlayerInput;
#else
        input = inputSource as IPlayerInput;
#endif
    }

    // ---- First（環境音）フェーズの文章 ----
    void ShowFirstMessage()
    {
        if (messageIndex < firstMessages.Count && dialogManager != null)
        {
            dialogManager.OpenDialog(firstMessages[messageIndex]);
            Debug.Log($"[TutorialManager] Firstメッセージ表示({messageIndex}): {firstMessages[messageIndex]}");
        }
        else
        {
            BeginAmbient();
        }
    }

    // ---- Second（異常音）フェーズの文章 ----
    void ShowSecondMessage()
    {
        if (messageIndex < secondMessages.Count && dialogManager != null)
        {
            dialogManager.OpenDialog(secondMessages[messageIndex]);
            Debug.Log($"[TutorialManager] Secondメッセージ表示({messageIndex}): {secondMessages[messageIndex]}");
        }
        else
        {
            BeginAnomaly();
        }
    }

    // ダイアログが閉じられたら、次の文章へ。全部閉じ終わったら音フェーズへ。
    void HandleDialogClosed()
    {
        if (step == Step.FirstText)
        {
            messageIndex++;
            ShowFirstMessage();
        }
        else if (step == Step.SecondText)
        {
            messageIndex++;
            ShowSecondMessage();
        }
    }

    void BeginAmbient()
    {
        step = Step.Ambient;
        forwardTimer = 0f;
        Debug.Log("[TutorialManager] 環境音フェーズ開始");

        if (!PlayCurrentEvent())
        {
            BeginSecondText();
        }
    }

    void BeginAnomaly()
    {
        step = Step.Anomaly;
        Debug.Log("[TutorialManager] 異常音フェーズ開始");

        if (!PlayCurrentEvent())
        {
            BeginTransition();
        }
    }

    void BeginSecondText()
    {
        step = Step.SecondText;
        messageIndex = 0;
        ShowSecondMessage();
    }

    // TutorialSequencerから次の音を取り出して再生する。失敗したら false。
    bool PlayCurrentEvent()
    {
        currentEvent = sequencer != null ? sequencer.Next() : null;
        if (currentEvent == null)
        {
            Debug.LogWarning("[TutorialManager] 音を取得できませんでした。");
            return false;
        }

        Debug.Log($"[TutorialManager] 音を再生: {currentEvent.name}（kind: {currentEvent.kind}）");

        if (audioSource != null && currentEvent.clip != null)
        {
            audioSource.pitch = currentEvent.pitch;
            audioSource.clip = currentEvent.clip;
            audioSource.Play();
        }

        return true;
    }

    void UpdateAmbient()
    {
        if (input == null) return;

        // 環境音：振り返らず前方を向き続けるのが正解。振り返ったらやり直し。
        if (input.IsTurnedBack)
        {
            forwardTimer = 0f;
            return;
        }

        forwardTimer += Time.deltaTime;
        if (forwardTimer >= forwardConfirmSeconds)
        {
            Debug.Log("[TutorialManager] 環境音の操作OK（まっすぐ歩いた）");
            currentEvent = null;
            BeginSecondText();
        }
    }

    void UpdateAnomaly()
    {
        if (input == null) return;

        // 異常音：振り返ってシャッターを撮るのが正解。
        bool turnedBack = input.IsTurnedBack;
        bool shutter = input.ShutterDown;
        if (shutter) input.ConsumeShutter();

        if (turnedBack && shutter)
        {
            Debug.Log("[TutorialManager] 異常音の操作OK（振り返り＋シャッター）");
            currentEvent = null;
            BeginTransition();
        }
    }

    void BeginTransition()
    {
        if (step == Step.Transition) return;
        step = Step.Transition;

        Debug.Log("[TutorialManager] 全フェーズ完了。MainGameへ遷移します。");

        StopSound();

        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.FadeToScene(nextSceneName);
        }
        else
        {
            Debug.LogError("[TutorialManager] FadeManager が見つかりません。", this);
        }
    }

    void StopSound()
    {
        if (audioSource == null) return;
        audioSource.Stop();
        audioSource.clip = null;
        audioSource.pitch = 1f;
    }
}
