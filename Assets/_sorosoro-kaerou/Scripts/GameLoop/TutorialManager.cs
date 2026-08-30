// TutorialManager.cs
// チュートリアル専用。各工程を順に進める。
//   Calibration   = キャリブレーション（スマホを正しい姿勢で構える）
//   First         = 歩き始める前のメッセージ（複数）
//   WalkStart     = 歩き始めた後のメッセージ（複数）
//   Second        = 環境音を鳴らした後のメッセージ（複数）
//   数秒歩く        = 環境音を無視してまっすぐ歩き続ける
//   BeforeAnomaly = 異常音が鳴った後のメッセージ（複数）
//   CheckPhoto    = メッセージ後、ButtonHandlerのクリックイベントを待って進行
//   AfterAnomaly  = 撮影成功後のメッセージ（複数）
//   その後、FadeManager で MainGame へ遷移する。
// 入力は IPlayerInput（Editor= inputSource、実機= AndroidManager）。
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SorosoroKaerou;
using SoroSoro.Events;

public sealed class TutorialManager : MonoBehaviour
{
    [Header("差し込み口")]
    [SerializeField] UIDialogManager dialogManager;
    [SerializeField] AudioSource audioSource;
    [SerializeField] MonoBehaviour inputSource;
    [SerializeField] SoundEventDefinition[] sequence; // 先頭=環境音、次=異常音
    [SerializeField] PlayerForwardWalker walker;      // 歩行制御用（未設定なら自動検出）

    [Header("キャリブレーション")]
    [SerializeField] GameObject calibrationCanvasPrefab;

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

    TutorialSequencer sequencer;
    IPlayerInput input;
    Step step;
    int messageIndex;
    float walkTimer;
    GameObject calibrationCanvasInstance;

    enum Step
    {
        Calibration,
        FirstText,
        WalkStartText,
        SecondText,
        Walking,
        BeforeAnomalyText,
        CheckPhoto,
        AfterAnomalyText,
        Transition
    }

    void OnEnable()
    {
        GameEvents.OnPhonePoseConfirmed += HandlePhonePoseConfirmed;
    }

    void OnDisable()
    {
        GameEvents.OnPhonePoseConfirmed -= HandlePhonePoseConfirmed;
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

        // キャリブレーション完了まで歩行を止める
        if (walker != null)
        {
            walker.enabled = false;
        }

        BeginCalibration();
    }

    void OnDestroy()
    {
        if (dialogManager != null)
        {
            dialogManager.OnClosedComplete.RemoveListener(HandleDialogClosed);
        }

        if (calibrationCanvasInstance != null)
        {
            Destroy(calibrationCanvasInstance);
            calibrationCanvasInstance = null;
        }

        if (ButtonHandler.Instance != null)
        {
            ButtonHandler.Instance.OnButtonClicked -= OnShutterClicked;
        }
    }

    void Update()
    {
        TryAcquireInput();

        if (step == Step.Walking)
        {
            UpdateWalking();
        }
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

    void BeginCalibration()
    {
        step = Step.Calibration;
        if (calibrationCanvasPrefab != null)
        {
            calibrationCanvasInstance = Instantiate(calibrationCanvasPrefab);
            Debug.Log("[TutorialManager] キャリブレーション開始");
        }
        else
        {
            // prefabが未設定の場合はスキップ
            Debug.LogWarning("[TutorialManager] calibrationCanvasPrefab が未設定のためスキップします。", this);
            BeginFirstText();
        }
    }

    void HandlePhonePoseConfirmed()
    {
        if (step != Step.Calibration) return;

        if (calibrationCanvasInstance != null)
        {
            Destroy(calibrationCanvasInstance);
            calibrationCanvasInstance = null;
        }
        Debug.Log("[TutorialManager] キャリブレーション完了");
        BeginFirstText();
    }

    void BeginFirstText()
    {
        step = Step.FirstText;
        messageIndex = 0;
        ShowOrAdvance(firstMessages, StartWalkingAndWalkStart);
    }

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

    // ---- シャッター（ボタン）入力待ち ----
    void BeginCheckPhoto()
    {
        step = Step.CheckPhoto;
        Debug.Log("[TutorialManager] 撮影確認フェーズ開始（ButtonHandlerの入力を待機します）");

        if (ButtonHandler.Instance != null)
        {
            // ボタンが押されたら OnShutterClicked を呼ぶように登録
            ButtonHandler.Instance.OnButtonClicked += OnShutterClicked;
        }
        else
        {
            Debug.LogError("[TutorialManager] ButtonHandler.Instance が見つかりません。自動で進行します。");
            BeginAfterAnomaly(); // エラーで進行不能になるのを防ぐ
        }
    }

    void OnShutterClicked()
    {
        // CheckPhotoフェーズ以外でボタンが押されても無視する
        if (step != Step.CheckPhoto) return;

        // 重複実行を防ぐため、イベントから自身を解除
        if (ButtonHandler.Instance != null)
        {
            ButtonHandler.Instance.OnButtonClicked -= OnShutterClicked;
        }

        Debug.Log("[TutorialManager] ボタン入力を検知。入力重複を防ぐため数フレーム待機します...");
        StartCoroutine(WaitAndBeginAfterAnomaly());
    }

    // 入力イベントが完全に消費されるのを待ってから次へ進むコルーチン
    IEnumerator WaitAndBeginAfterAnomaly()
    {
        // メッセージの即時スキップ（吸われ）を防ぐため3フレーム待機
        yield return null;
        yield return null;
        yield return null;

        BeginAfterAnomaly();
    }

    void BeginAfterAnomaly()
    {
        step = Step.AfterAnomalyText;
        messageIndex = 0;
        Debug.Log("[TutorialManager] AfterAnomalyフェーズ開始");
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