// SoundEventDefinition.cs
using UnityEngine;

[CreateAssetMenu(fileName = "SoundEvent", menuName = "SoroSoro/SoundEvent")]
public sealed class SoundEventDefinition : ScriptableObject
{
    [Tooltip("再生する効果音")]
    public AudioClip clip;

    [Tooltip("音の種類（環境音 / 異常音）")]
    public SoundKind kind;

    [Tooltip("判断が難しい紛らわしい音か")]
    public bool isConfusing;

    [Tooltip("再生ピッチ")]
    [Range(0.5f, 2f)]
    public float pitch = 1f;

    [Tooltip("フラッシュ撮影が成立したときに見せる正体画像。異常音／環境音どちらにも設定可（環境音で未設定ならnullのまま＝何も写らない）")]
    public Sprite ghostSprite;
}