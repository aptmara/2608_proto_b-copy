using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenu("Post-processing/Custom/Dark Room Effect")]
public class DarkRoomEffect : VolumeComponent, IPostProcessComponent
{
    [Tooltip("暗闇の色(光が届かない範囲)")]
    public ColorParameter baseColor = new ColorParameter(Color.black);

    [Tooltip("光源の色")]
    public ColorParameter lightColor = new ColorParameter(new Color(1f, 0.9f, 0.7f, 1f));

    [Tooltip("光源からの最大到達距離")]
    public MinFloatParameter lightRadius = new MinFloatParameter(45f, 0f);

    [Tooltip("減衰カーブの鋭さ(大きいほど光源付近に明るさが集中し、外側は速く暗くなる)")]
    public MinFloatParameter softness = new MinFloatParameter(3f, 0.01f);

    [Tooltip("懐中電灯のように照らす範囲の半角(度)")]
    public ClampedFloatParameter spotAngle = new ClampedFloatParameter(45f, 1f, 179f);

    [Tooltip("光の中に浮かぶ塵のような粒子の出現しやすさ(寿命10秒、ごく低い値推奨)")]
    public ClampedFloatParameter particleIntensity = new ClampedFloatParameter(0.05f, 0f, 1f);

    public bool IsActive() => active;

    public bool IsTileCompatible() => false;
}
