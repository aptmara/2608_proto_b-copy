// JudgeContext.cs
using UnityEngine;

public readonly struct JudgeContext
{
    [field: Tooltip("鳴った音の種別")]
    public SoundKind kind { get; init; }

    [field: Tooltip("振り返りを行ったか")]
    public bool didTurn { get; init; }

    [field: Tooltip("撮影（シャッター）を行ったか")]
    public bool didShutter { get; init; }

    [field: Tooltip("振り返った時点で電池が空だったか")]
    public bool batteryWasEmptyOnAim { get; init; }

    [field: Tooltip("撮影した時点で電池が空だったか")]
    public bool batteryWasEmptyOnShutter { get; init; }
}