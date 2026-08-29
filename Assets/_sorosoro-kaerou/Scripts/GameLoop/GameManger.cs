// GameManager.cs
using UnityEngine;
using SorosoroKaerou;
using SoroSoro.Events;

public sealed class GameManager : MonoBehaviour
{
    [Header("差し込み口")]
    [SerializeField] MonoBehaviour inputSource;
    [SerializeField] MonoBehaviour feedbackSource;

    [Header("設定")]
    [SerializeField] DayConfig[] dayConfigs;
    [SerializeField] GameBalanceConfig balance;

    IPlayerInput input;
    IFeedbackPresenter feedback;
    BatteryModel battery;

    GameState state;
    DayProgress progress;
    DayCounter dayCounter;
    SoundEventPlayer soundPlayer;
    JudgeWindow judgeWindow;

    bool wasTurnedBack;

    void Awake()
    {
        input = inputSource as IPlayerInput;
        feedback = feedbackSource as IFeedbackPresenter;
        
        state = new GameState();
        progress = new DayProgress();
        dayCounter = new DayCounter(dayConfigs);
        soundPlayer = new SoundEventPlayer(balance, new System.Random());
        judgeWindow = new JudgeWindow();
        
        // TODO: BatteryModelの初期化（GameBalanceConfigが必要）
    }

    void Update()
    {
        if (state.Phase is PhaseKind.Title or PhaseKind.GameOver or PhaseKind.Result) return;

        float dt = Time.deltaTime;
        state.SyncInput(input);

        if (state.IsTurnedBack && battery != null && !battery.IsEmpty)
        {
            battery.DrainLight(dt);
        }

        if (feedback != null && battery != null)
        {
            feedback.SetLight(state.IsTurnedBack && !battery.IsEmpty);
        }

        UpdateAimEvents();

        bool forward = state.IsWalkingForward;
        progress.Tick(dt, forward);
        soundPlayer.Tick(dt, forward);
        
        judgeWindow.Tick(dt);

        GameEvents.RaiseProgressChanged(progress.Normalized);

        if (judgeWindow.IsOpen)
        {
            if (input != null && input.ShutterDown)
            {
                // TODO: 判定処理の実装
            }
            else if (judgeWindow.IsExpired)
            {
                // TODO: 時間切れ処理の実装
            }
        }
        else if (soundPlayer.CanFire(0f))
        {
            var def = soundPlayer.Fire();
            if (def != null)
            {
                state.SetPhase(PhaseKind.Judging);
                state.SetCurrentEvent(def);
                judgeWindow.Open(3f); // TODO: configから取得
                GameEvents.RaiseSoundPlayed(def.kind);
                GameEvents.RaiseJudgeWindowOpened(3f);
            }
        }

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

    void EndDay()
    {
        state.SetPhase(PhaseKind.DayClear);
        GameEvents.RaiseDayCleared(dayCounter.CurrentDay);
        dayCounter.Advance();
    }
}