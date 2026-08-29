using SorosoroKaerou;
using UnityEngine;

public class AndroidManager : Singleton<AndroidManager>
{
    public IPlayerInput PlayerInput { get; private set; }
    public IFeedbackPresenter Feedback { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        PlayerInput = GetComponent<AndroidPlayerInput>();
        Feedback = GetComponent<AndroidFeedbackPresenter>();
    }
}