using UnityEngine;
using SoroSoro.Events;

public sealed class DayClearContinueButton : MonoBehaviour
{
    public void OnClickContinue() => GameEvents.RaiseDayClearContinue();
}