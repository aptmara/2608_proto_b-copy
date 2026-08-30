// TutorialManager.cs
// チュートリアル専用。各工程を順に進める。
//   First         = 歩き始める前のメッセージ（複数）
//   WalkStart     = 歩き始めた後のメッセージ（複数）
//   Second        = 環境音を鳴らした後のメッセージ（複数）
//   数秒歩く       = 環境音を無視してまっすぐ歩き続ける
//   BeforeAnomaly = 異常音が鳴った後のメッセージ（複数）
//   CheckPhoto    = メッセージ後、振り返ってシャッターを切る（分岐せず無条件で進行）
//   AfterAnomaly  = 撮影成功後のメッセージ（複数）
//   その後、FadeManager で MainGame へ遷移する。
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
    [SerializeField] PlayerForwardWalker walker;      // 歩行制御用（未設定なら自動検出）

    [Header("メッセージ（各工程で複数回表示）")]
    [SerializeField] List<string> firstMessages = new List<string>();
    [SerializeField] List<string> walkStartMessages = new List<string>();
    [SerializeField] List<string> secondMessages = new List<string>();
    [SerializeField] List<string> beforeAnomalyMessages = new List<string>();
    [SerializeField] List<string> afterAnomalyMessages = new List<string>();

    [Header("遷移")]
    [SerializeField] string nextSceneName = "MainGame";

    [Header("判定")]
    [SerializeField] float walkSeconds = 1.5f;
    [SerializeField] float failSeconds = 2.0f; // 異常音後に前を向いたままの場合の猶予時間（※今回の修正では未使用になりますが互換性のため残しています）

    TutorialSequencer sequencer;
    IPlayerInput input;
    Step step;
    int messageIndex;
    float walkTimer;
    float failTimer; // 失敗判定用タイマー

    enum Step
    {
        FirstText,
        WalkStartText,
        SecondText,
        Walking,
        BeforeAnomalyText,
        CheckPhoto,
        AfterAnomalyText,
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

        if (walker == null)
        {
            walker = FindFirstObjectByType<PlayerForwardWalker>();
        }

        // 歩き始める前なので、一旦歩行を止めておく
        if (walker != null)
        {
            walker.enabled = false;
        }

        step = Step.FirstText;
        messageIndex = 0;
        ShowOrAdvance(firstMessages, StartWalkingAndWalkStart);
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

        if (step == Step.Walking) UpdateWalking();
        else if (step == Step.CheckPhoto) UpdateCheckPhoto();
    }

    void TryAcquireInput()
    {
        if (input != null) return;
        input = AndroidManager.Instance.PlayerInput;
    }

    // メッセージ表示（リスト末尾まで達したら onFinished を呼ぶ）
    void ShowOrAdvance(List<string> list, System.Action onFinished)
    {
        if (messageIndex < list.Count && dialogManager != null)
        {
            dialogManager.OpenDialog(list[messageIndex]);
            Debug.Log($"[TutorialManager] メッセージ表示({messageIndex}): {list[messageIndex]}");
        }
        else
        {
            onFinished();
        }
    }

    void HandleDialogClosed()
    {
        messageIndex++;

        switch (step)
        {
            case Step.FirstText:
                ShowOrAdvance(firstMessages, StartWalkingAndWalkStart);
                break;
            case Step.WalkStartText:
                ShowOrAdvance(walkStartMessages, PlayAmbientAndSecond);
                break;
            case Step.SecondText:
                ShowOrAdvance(secondMessages, BeginWalking);
                break;
            case Step.BeforeAnomalyText:
                ShowOrAdvance(beforeAnomalyMessages, BeginCheckPhoto);
                break;
            case Step.AfterAnomalyText:
                ShowOrAdvance(afterAnomalyMessages, BeginTransition);
                break;
        }
    }

    // ---- 工程遷移 ----
    void StartWalkingAndWalkStart()
    {
        if (walker != null) walker.enabled = true; // 歩き始める
        step = Step.WalkStartText;
        messageIndex = 0;
        Debug.Log("[TutorialManager] 歩き始めました（WalkStart）");
        ShowOrAdvance(walkStartMessages, PlayAmbientAndSecond);
    }

    void PlayAmbientAndSecond()
    {
        step = Step.SecondText;
        messageIndex = 0;
        Debug.Log("[TutorialManager] 環境音を鳴らします");
        PlayCurrentEvent(); // 環境音
        ShowOrAdvance(secondMessages, BeginWalking);
    }

    void BeginWalking()
    {
        step = Step.Walking;
        walkTimer = 0f;
        Debug.Log("[TutorialManager] 数秒歩くフェーズ開始");
    }

    void PlayAnomalyAndBeforeText()
    {
        step = Step.BeforeAnomalyText;
        messageIndex = 0;
        Debug.Log("[TutorialManager] 異常音を鳴らします");
        PlayCurrentEvent(); // 異常音
        Debug.Log("[TutorialManager] BeforeAnomalyフェーズ開始");
        ShowOrAdvance(beforeAnomalyMessages, BeginCheckPhoto);
    }

    void BeginCheckPhoto()
    {
        step = Step.CheckPhoto;
        failTimer = 0f; // 判定タイマーリセット
        Debug.Log("[TutorialManager] 撮影確認フェーズ開始（自動成功）");
    }

    void BeginAfterAnomaly()
    {
        step = Step.AfterAnomalyText;
        messageIndex = 0;
        Debug.Log("[TutorialManager] 撮影成功。AfterAnomalyフェーズ開始");
        ShowOrAdvance(afterAnomalyMessages, BeginTransition);
    }

    // ---- 操作確認 ----
    void UpdateWalking()
    {
        if (input == null) return;

        // 環境音を無視してまっすぐ歩き続けるのが正解。振り返ったらやり直し。
        if (input.IsTurnedBack)
        {
            walkTimer = 0f;
            return;
        }

        walkTimer += Time.deltaTime;
        if (walkTimer >= walkSeconds)
        {
            Debug.Log("[TutorialManager] 数秒歩いた（まっすぐ歩行OK）");
            PlayAnomalyAndBeforeText();
        }
    }

    void UpdateCheckPhoto()
    {
        if (input == null) return;

        bool shutter = input.ShutterDown;
        if (shutter) input.ConsumeShutter();

        // 失敗判定を行わず、任意のタイミング（またはシャッター入力等）で無条件に成功扱いへ進める場合、
        // ここではシャッターが押されたタイミング、あるいは即座に成功扱いに移行させます。
        // 例として「シャッターが押されたら成功」とする場合は以下のようにします（自動ですぐに進めたい場合は shutter の判定を外してください）。
        if (shutter)
        {
            Debug.Log("[TutorialManager] 撮影成功（無条件進行）");
            BeginAfterAnomaly();
        }
    }

    // ---- 音 ----
    void PlayCurrentEvent()
    {
        var def = sequencer != null ? sequencer.Next() : null;
        if (def == null)
        {
            Debug.LogWarning("[TutorialManager] 音を取得できませんでした。");
            return;
        }

        Debug.Log($"[TutorialManager] 音を再生: {def.name}（kind: {def.kind}）");

        if (audioSource != null && def.clip != null)
        {
            audioSource.pitch = def.pitch;
            audioSource.clip = def.clip;
            audioSource.Play();
        }
    }

    // ---- 遷移 ----
    void BeginTransition()
    {
        if (step == Step.Transition) return;
        step = Step.Transition;

        Debug.Log("[TutorialManager] 全工程完了。MainGameへ遷移します。");

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