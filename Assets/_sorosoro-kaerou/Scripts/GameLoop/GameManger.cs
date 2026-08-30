// GameManager.cs
using UnityEngine;
using SorosoroKaerou;
using SoroSoro.Events;

public sealed class GameManager : MonoBehaviour
{
    [Header("差し込み口")]
    [SerializeField] MonoBehaviour inputSource;      // IPlayerInputを実装したもの
    [SerializeField] MonoBehaviour feedbackSource;   // IFeedbackPresenterを実装したもの
    [SerializeField] AudioSource audioSource;        // 音イベント再生用（任意）

    [Header("設定")]
    [SerializeField] DayConfig[] dayConfigs;         // 先頭にDay0を置く
    [SerializeField] GameBalanceConfig balance;

    [Header("キャリブレーション")]
    [SerializeField] GameObject calibrationCanvasPrefab;

    IPlayerInput input;
    IFeedbackPresenter feedback;
    BatteryModel battery;

    GameState state;
    DayProgress progress;
    DayCounter dayCounter;
    SoundEventPlayer soundPlayer;
    JudgeWindow judgeWindow;
    JudgeResolver resolver;
    IDaySequencer sequencer;
    System.Random random;

    // dayStatsはBeginDayで、totalStatsはStartGameでリセットする
    DayStats dayStats;
    DayStats totalStats;
    bool wasTurnedBack;
    float lookBackTimer;
    bool hasRaisedLookBackHeld;
    bool wasWalkingForward;

    GameObject calibrationCanvasInstance;
    bool isCalibrating;

    // 空振り硬直
    // 撮影後の停止（演出待機 兼 空振り硬直）
    // balance.stagingWaitTime を0にすれば演出待機分は消え、従来の空振り硬直だけが残る
    float holdTimer;
    bool isHolding => holdTimer > 0f;

    // 演出側・デバッグ表示から停止中かどうかを参照するための入口
    public bool IsHolding => isHolding;
    public bool IsCalibrating => isCalibrating;

    bool waitingContinue;

    void Awake()
    {
        Debug.Assert(balance != null, "GameBalanceConfigが未設定です", this);
        Debug.Assert(dayConfigs != null && dayConfigs.Length > 0, "DayConfigが未設定です", this);

        random = new System.Random();
        state = new GameState();
        progress = new DayProgress();
        dayCounter = new DayCounter(dayConfigs);
        judgeWindow = new JudgeWindow();
        resolver = new JudgeResolver();
        battery = new BatteryModel(balance);
        soundPlayer = new SoundEventPlayer(balance, random);
        dayStats = new DayStats();
        totalStats = new DayStats();
    }

    void OnEnable()
    {
        GameEvents.OnStagingFinished += NotifyStagingFinished;
        GameEvents.OnPhonePoseConfirmed += HandlePhonePoseConfirmed;
    }

    void OnDisable()
    {
        GameEvents.OnStagingFinished -= NotifyStagingFinished;
        GameEvents.OnPhonePoseConfirmed -= HandlePhonePoseConfirmed;
    }

    void OnDestroy()
    {
        if (calibrationCanvasInstance != null)
        {
            Destroy(calibrationCanvasInstance);
            calibrationCanvasInstance = null;
        }
    }

    void Start()
    {
        TryAcquireDependencies();
        StartGame();
    }

    // AndroidManager側のAwake()がこちらより先に走っている保証がないため、
    // 取得できるまでUpdateから毎フレーム呼び直す
    void TryAcquireDependencies()
    {
        if (input != null && feedback != null) return;

#if UNITY_ANDROID && !UNITY_EDITOR
        // Android実機ではAndroidManagerから取得
        input = AndroidManager.Instance.PlayerInput;
        feedback = AndroidManager.Instance.Feedback;
#else
        // Editor / その他の環境ではInspectorから取得
        input = inputSource as IPlayerInput;
        feedback = feedbackSource as IFeedbackPresenter;
#endif
    }

    public void StartGame()
    {
        dayCounter.Reset();
        totalStats.Reset();
        holdTimer = 0f;
        BeginDay(dayCounter.CurrentConfig);
    }

    // ---------------------------------------------------------------
    // 1日の開始
    // ---------------------------------------------------------------
    void BeginDay(DayConfig config)
    {
        StopSound();
        if (config == null) return;

        battery.Refill();
        holdTimer = 0f;
        dayStats.Reset();
        progress.Reset(config.requiredWalkSeconds);
        judgeWindow.Close();
        state.ClearCurrentEvent();
        wasTurnedBack = false;
        lookBackTimer = 0f;
        hasRaisedLookBackHeld = false;
        wasWalkingForward = false;

        // Day0（fixedSequenceあり）はTutorialSequencer、本編はRandomSequencer
        sequencer = (config.fixedSequence != null && config.fixedSequence.Length > 0)
            ? new TutorialSequencer(config.fixedSequence)
            : new RandomSequencer(config, random);

        soundPlayer.SetSequencer(sequencer);

        // Day0かどうかでPhaseを分岐させない。
        // GameManagerが持つDay0固有の分岐はDayConfigの2フラグ（allowGameOver / countScore）だけに限定する。
        // PhaseKind.Day0を使うと GameState.IsWalkingForward（Walking / Judgingのみ許可）から外れ、
        // 進行度・音イベントのTickが一切進まなくなるため、ここではWalkingに統一する。
        state.SetPhase(PhaseKind.Walking);

        GameEvents.RaiseDayStarted(dayCounter.CurrentDay);
        GameEvents.RaiseBatteryChanged(battery.Normalized);
        GameEvents.RaiseProgressChanged(progress.Normalized);

        if (calibrationCanvasPrefab != null)
        {
            if (calibrationCanvasInstance != null)
            {
                Destroy(calibrationCanvasInstance);
            }
            calibrationCanvasInstance = Instantiate(calibrationCanvasPrefab);
            isCalibrating = true;
        }
        else
        {
            isCalibrating = false;
        }
    }

    /// <summary>
    /// キャリブレーション完了時の処理。
    /// </summary>
    void HandlePhonePoseConfirmed()
    {
        if (calibrationCanvasInstance != null)
        {
            Destroy(calibrationCanvasInstance);
            calibrationCanvasInstance = null;
        }
        isCalibrating = false;
    }

    // ---------------------------------------------------------------
    // 毎フレーム処理（詳細クラス図6章の順序を厳守）
    // ---------------------------------------------------------------
    void Update()
    {
        if (isCalibrating) return;
        if (state.Phase is PhaseKind.Title or PhaseKind.GameOver or PhaseKind.Result or PhaseKind.DayClear) return;
        if (input == null || feedback == null) TryAcquireDependencies();
        if (input == null || feedback == null) return;

        float dt = Time.deltaTime;

        // 1. 入力を読む
        state.SyncInput(input);

        // ShutterDownはウィンドウの開閉に関わらず毎フレーム読み切って即消費する。
        // HandleJudgingの中だけで読むと、ウィンドウが閉じている間に来た入力が
        // 消費されずに残り、次のウィンドウで誤検出されるため。
        bool shutterDown = input.ShutterDown;
        if (shutterDown) input.ConsumeShutter();

        // 2. 電池を減らす（ライト制御より先）
        // 撮影後の停止中は減らさない。プレイヤーが操作している時間ではないため。
        if (state.IsTurnedBack && !battery.IsEmpty && !isHolding)
        {
            battery.DrainLight(dt);
            GameEvents.RaiseBatteryChanged(battery.Normalized);
        }

        // 3. ライトを反映（電池切れならfalse）
        feedback.SetLight(state.IsTurnedBack && !battery.IsEmpty);

        // 4. 振り返り開始・終了イベント
        UpdateAimEvents(dt);

        // 撮影後の停止中は歩行・判定・音イベントを進めない。
        // ライト（3.）はこの手前で反映済みなので、停止中も点いたままになる。
        // 暗転すると演出の写真が見えなくなるため、これは意図した例外。
        if (isHolding)
        {
            holdTimer -= dt;
            if (holdTimer <= 0f) EndHold();
            return;
        }

        // 5. 各種タイマー
        bool forward = state.IsWalkingForward;
        progress.Tick(dt, forward);
        soundPlayer.Tick(dt, forward);
        judgeWindow.Tick(dt); // 常に減算

        if (forward != wasWalkingForward)
        {
            wasWalkingForward = forward;
            GameEvents.RaiseWalkingChanged(forward);
        }

        GameEvents.RaiseProgressChanged(progress.Normalized);

        // 6. 判定処理またはイベント発火
        if (judgeWindow.IsOpen)
        {
            HandleJudging(shutterDown);
        }
        else if (soundPlayer.CanFire(RemainToDayEnd()))
        {
            FireSoundEvent();
        }

        // 7. 進行度満了は最後
        if (progress.IsCompleted && !judgeWindow.IsOpen)
        {
            EndDay();
        }
    }

    void UpdateAimEvents(float dt)
    {
        if (state.IsTurnedBack && !wasTurnedBack)
        {
            GameEvents.RaiseAimStarted();
            lookBackTimer = 0f;
            hasRaisedLookBackHeld = false;
        }

        if (!state.IsTurnedBack && wasTurnedBack)
        {
            GameEvents.RaiseAimEnded();
            lookBackTimer = 0f;
            hasRaisedLookBackHeld = false;
        }

        wasTurnedBack = state.IsTurnedBack;

        if (state.IsTurnedBack && !isHolding)
        {
            lookBackTimer += dt;
            float threshold = balance != null ? balance.lookBackDurationThreshold : 2f;
            if (lookBackTimer >= threshold && !hasRaisedLookBackHeld)
            {
                hasRaisedLookBackHeld = true;
                GameEvents.RaiseLookBackHeld();
            }
        }
        else
        {
            lookBackTimer = 0f;
            hasRaisedLookBackHeld = false;
        }
    }

    float RemainToDayEnd()
    {
        var config = dayCounter.CurrentConfig;
        if (config == null) return 0f;
        return config.requiredWalkSeconds * (1f - progress.Normalized);
    }

    // ---------------------------------------------------------------
    // 音イベント発火
    // ---------------------------------------------------------------
    void FireSoundEvent()
    {
        var def = soundPlayer.Fire();
        if (def == null) return;

        var config = dayCounter.CurrentConfig;

        state.SetPhase(PhaseKind.Judging);
        state.SetCurrentEvent(def);

        if (audioSource != null && def.clip != null)
        {
            audioSource.pitch = def.pitch;
            audioSource.clip = def.clip;
            audioSource.Play();
        }

        judgeWindow.Open(config.judgeWindowDuration);

        GameEvents.RaiseSoundPlayed(def.kind);
        GameEvents.RaiseJudgeWindowOpened(config.judgeWindowDuration);
        GameEvents.RaiseGhostAppeared();
    }

    // ---------------------------------------------------------------
    // 判定受付中の処理（4象限の入口）
    // ---------------------------------------------------------------
    void HandleJudging(bool shutterDown)
    {
        // シャッターはウィンドウが開いている間だけ拾う
        if (shutterDown)
        {
            bool flashSucceeded = battery.TryConsumeFlash();
            feedback.Flash();
            GameEvents.RaiseBatteryChanged(battery.Normalized);

            CloseJudge(BuildContext(didShutter: true, flashSucceeded: flashSucceeded));
            return;
        }

        if (judgeWindow.IsExpired)
        {
            CloseJudge(BuildContext(didShutter: false, flashSucceeded: false));
        }
    }

    JudgeContext BuildContext(bool didShutter, bool flashSucceeded)
    {
        return new JudgeContext
        {
            kind = state.CurrentEvent.kind,
            didTurn = state.HasAimedThisEvent,
            didShutter = didShutter,
            batteryWasEmptyOnAim = battery.IsEmpty,
            batteryWasEmptyOnShutter = didShutter && !flashSucceeded,
        };
    }

    // ---------------------------------------------------------------
    // 判定確定（4象限の出口）
    // ---------------------------------------------------------------
    void CloseJudge(in JudgeContext ctx)
    {
        JudgeResult result = resolver.Resolve(ctx);
        judgeWindow.Close();

        var config = dayCounter.CurrentConfig;

        if (config.countScore)
        {
            dayStats.Add(result);
            totalStats.Add(result);
        }

        GameEvents.RaiseJudged(result);
        GameEvents.RaiseAimEnded();
        wasTurnedBack = false;
        lookBackTimer = 0f;
        hasRaisedLookBackHeld = false;

        if (result == JudgeResult.Missed)
        {
            // 判定失敗時だけ無条件でRetry()を呼ぶ。
            // 本編（RandomSequencer）ではRetry()は空実装なので実質何もしない。
            // Day0（TutorialSequencer）ではここで初めて「同じ音をもう一度鳴らす」予約が入る。
            // 成功時に呼んでしまうとindexが進む前に再生予約が立ち、
            // 正解してもいつまでも同じ音がループし続けるバグになるため、必ずMissedの中だけに限定する。
            sequencer.Retry();

            if (config.allowGameOver)
            {
                GameOver(resolver.ResolveReason(ctx));
                return;
            }

            // Day0など失敗を許容する場合は歩行へ戻すだけ
            state.SetPhase(PhaseKind.Walking);
            state.ClearCurrentEvent();
            return;
        }

        // 撮影したケース（Repelled / Wasted）だけ停止する。
        // 「怪奇のときだけ止める」にしないのは、判定結果でロジックとUIの分岐を増やさず
        // 「シャッターを押したら止まる」の1行でルールを閉じるため。
        float hold = ctx.didShutter ? balance.stagingWaitTime : 0f;
        if (result == JudgeResult.Wasted) hold = Mathf.Max(hold, balance.wastedStunTime);

        // 撮影した全ケースで発火し、幽霊が写っていなければnullを渡す。
        // 停止するケースと1:1にしてあるため、UI側は「来たら出す、消えたら止まりが明ける」だけで済む。
        // state.ClearCurrentEvent()より前に呼ぶ必要がある（PickGhostSpriteがCurrentEventを参照するため）。
        if (ctx.didShutter)
        {
            GameEvents.RaisePhotoCaptured(PickGhostSprite());
            StopSound();
        }

        BeginHold(hold);
        state.ClearCurrentEvent();
    }

    // ---------------------------------------------------------------
    // 撮影された幽霊画像の決定
    // ---------------------------------------------------------------
    Sprite PickGhostSprite()
    {
        var def = state.CurrentEvent;
        if (def == null) return null;
        if (def.ghostSprite != null) return def.ghostSprite;
        if (def.kind != SoundKind.Anomaly) return null;

        var pool = balance.fallbackGhostSprites;
        if (pool == null || pool.Length == 0) return null;

        return pool[random.Next(pool.Length)];
    }

    // ---------------------------------------------------------------
    // 撮影後の停止
    // ---------------------------------------------------------------
    void BeginHold(float seconds)
    {
        // stagingWaitTime = 0 かつ空振りでない場合はここを通り、演出待機の導入前と同じ即時復帰になる
        if (seconds <= 0f)
        {
            state.SetPhase(PhaseKind.Walking);
            return;
        }

        holdTimer = seconds;
        state.SetPhase(PhaseKind.Judging); // 停止中は歩けない扱い
    }

    void EndHold()
    {
        holdTimer = 0f;
        state.SetPhase(PhaseKind.Walking);
    }

    /// <summary>
    /// 演出側から停止を打ち切るための入口。
    /// 段階1では未使用だが、段階2でUIのフラグを繋ぐ際はここを呼ぶだけで済む。
    /// 呼ばれなくても holdTimer の満了で必ず復帰するため、UI未実装でも進行は止まらない。
    /// </summary>
    public void NotifyStagingFinished()
    {
        if (isHolding) EndHold();
    }

    // ---------------------------------------------------------------
    // 日の終了 → その日のリザルト → 次の日へ
    //
    // GameClearResultシーンのロード・アンロードはGameClearSceneTransitionの責務。
    // ここでは操作を止めて OnDayClearContinue を待つだけに留める。
    // 両方でLoadSceneAsyncを呼ぶとシーンが二重に積まれるため注意。
    // ---------------------------------------------------------------
    void EndDay()
    {
        if (calibrationCanvasInstance != null)
        {
            Destroy(calibrationCanvasInstance);
            calibrationCanvasInstance = null;
        }
        isCalibrating = false;

        StopSound();
        state.SetPhase(PhaseKind.DayClear);
        feedback.SetLight(false);

        dayStats.BatteryRemaining = battery.Normalized;

        // dayStatsは直後のBeginDayでResetされるため、値をコピーして渡す
        var clearResult = new ResultData
        {
            ReachedDay = dayCounter.CurrentDay,
            RepelledCount = dayStats.Repelled,
            TotalWasted = dayStats.Wasted,
            TotalCorrect = dayStats.Correct,
            BatteryRemaining = dayStats.BatteryRemaining,
            // Reason は日クリア時には使用しない（ゲームオーバー時のみ使用）
        };

        GameEvents.RaiseDayCleared(clearResult);
        StartCoroutine(DayClearRoutine());
    }

    System.Collections.IEnumerator DayClearRoutine()
    {
        waitingContinue = true;
        GameEvents.OnDayClearContinue += OnContinue;
        while (waitingContinue) yield return null;
        GameEvents.OnDayClearContinue -= OnContinue;

        // 最終DayConfigまで到達済みならAdvance()は何もしないため、同じ日を繰り返す
        dayCounter.Advance();
        BeginDay(dayCounter.CurrentConfig);
    }

    void OnContinue() => waitingContinue = false;

    // ---------------------------------------------------------------
    // ゲームオーバー
    // ---------------------------------------------------------------
    void GameOver(GameOverReason reason)
    {
        if (calibrationCanvasInstance != null)
        {
            Destroy(calibrationCanvasInstance);
            calibrationCanvasInstance = null;
        }
        isCalibrating = false;

        StopSound();
        state.SetPhase(PhaseKind.GameOver);
        feedback.SetLight(false);

        totalStats.BatteryRemaining = battery.Normalized;

        var data = new ResultData
        {
            ReachedDay = dayCounter.CurrentDay,
            RepelledCount = totalStats.Repelled,
            TotalWasted = totalStats.Wasted,
            TotalCorrect = totalStats.Correct,
            BatteryRemaining = totalStats.BatteryRemaining,
            Reason = reason,
        };

        GameEvents.RaiseGameOver(data);
    }

    // ---------------------------------------------------------------
    // リトライ（タイトルから再スタート）
    // ---------------------------------------------------------------
    public void Retry()
    {
        // DayClear待機中にRetryされた場合にコルーチンが残らないよう明示的に止める
        StopAllCoroutines();
        GameEvents.OnDayClearContinue -= OnContinue;
        waitingContinue = false;

        if (calibrationCanvasInstance != null)
        {
            Destroy(calibrationCanvasInstance);
            calibrationCanvasInstance = null;
        }
        isCalibrating = false;

        wasTurnedBack = false;
        lookBackTimer = 0f;
        hasRaisedLookBackHeld = false;
        holdTimer = 0f;
        StartGame();
    }

    // ---------------------------------------------------------------
    // 音の消去
    // ---------------------------------------------------------------
    void StopSound()
    {
        if (audioSource == null) return;
        audioSource.Stop();
        audioSource.clip = null;
        audioSource.pitch = 1f;
    }
}