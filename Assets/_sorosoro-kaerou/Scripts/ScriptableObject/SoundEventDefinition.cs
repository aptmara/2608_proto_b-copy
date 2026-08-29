// SoundEventDefinition.cs
using UnityEngine;

[CreateAssetMenu(fileName = "SoundEventDefinition", menuName = "SoroSoro/SoundEventDefinition")]
public sealed class SoundEventDefinition : ScriptableObject
{
    [Tooltip("再生する音イベントのAudioClip")]
    public AudioClip clip;

    [Tooltip("環境音（無視）か異常音（撃退）か")]
    public SoundKind kind;

    [Tooltip("難度調整用のフラグ（環境音と紛らわしい音など）")]
    public bool isConfusing;
}