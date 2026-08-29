using SorosoroKaerou;
using UnityEngine;

public class AndroidFeedbackPresenter : MonoBehaviour, IFeedbackPresenter
{
    private AndroidTorch torch;

    private void Awake()
    {
        torch = GetComponent<AndroidTorch>();
    }

    public void SetLight(bool on)
    {
        torch.SetTorch(on);
    }

    public void Flash()
    {
        torch.FlashAndSpawn();
    }

    public void Vibrate()
    {
        Handheld.Vibrate();
    }
}