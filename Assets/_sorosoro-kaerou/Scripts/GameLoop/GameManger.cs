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

    // 空振り硬直
    float stunTimer;
    bool isStunned => stunTimer > 0f;

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
        stunTimer = 0f;
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
        dayStats.Reset();
        progress.Reset(config.requiredWalkSeconds);
        judgeWindow.Close();
        state.ClearCurrentEvent();

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

        GameEvents.RaiseDayStarted(config.day);
        GameEvents.RaiseBatteryChanged(battery.Normalized);
        GameEvents.RaiseProgressChanged(progress.Normalized);
    }

    // ---------------------------------------------------------------
    // 毎フレーム処理（詳細クラス図6章の順序を厳守）
    // ---------------------------------------------------------------
    void Update()
    {
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
        if (state.IsTurnedBack && !battery.IsEmpty)
        {
            battery.DrainLight(dt);
            GameEvents.RaiseBatteryChanged(battery.Normalized);
        }

        // 3. ライトを反映（電池切れならfalse）
        feedback.SetLight(state.IsTurnedBack && !battery.IsEmpty);

        // 4. 振り返り開始・終了イベント
        UpdateAimEvents();

        // 空振り硬直中は歩行・判定を進めない
        if (isStunned)
        {
            stunTimer -= dt;
            if (stunTimer <= 0f)
            {
                stunTimer = 0f;
                state.SetPhase(PhaseKind.Walking);
            }
            return;
        }

        // 5. 各種タイマー
        bool forward = state.IsWalkingForward;
        progress.Tick(dt, forward);
        soundPlayer.Tick(dt, forward);
        judgeWindow.Tick(dt); // 常に減算

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

    void UpdateAimEvents()
    {
        if (state.IsTurnedBack && !wasTurnedBack) GameEvents.RaiseAimStarted();
        if (!state.IsTurnedBack && wasTurnedBack) GameEvents.RaiseAimEnded();
        wasTurnedBack = state.IsTurnedBack;
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

        if (result == JudgeResult.Wasted)
        {
            stunTimer = balance.wastedStunTime;
            state.SetPhase(PhaseKind.Judging); // 硬直中は歩けない扱い
        }
        else
        {
            state.SetPhase(PhaseKind.Walking);
        }

        state.ClearCurrentEvent();
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

        wasTurnedBack = false;
        stunTimer = 0f;
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