using SoroSoro.Events;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// ライト位置およびポストプロセスパラメータをシェーダーへ同期するクラス。
/// </summary>
[ExecuteInEditMode]
public class SyncLightPosition : MonoBehaviour
{
    [Header("端末ライトON時のブースト")]
    [SerializeField] private float lightOnRadiusMultiplier = 1.4f;
    [SerializeField] private float lightOnColorMultiplier = 1.3f;

    private bool isLightOn;

    private void OnEnable()
    {
        GameEvents.OnLightOnChanged += HandleLightOnChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnLightOnChanged -= HandleLightOnChanged;
    }

    private void HandleLightOnChanged(bool on)
    {
        isLightOn = on;
    }

    void Update()
    {
        // "_LightPos"を読み込むすべてのシェーダーに現在地を送信
        Shader.SetGlobalVector("_LightPos", transform.position);

        // アタッチされているオブジェクト(カメラ)のフォワード方向を送信(懐中電灯の向き)
        Shader.SetGlobalVector("_LightDir", transform.forward);

        // Volumeで設定したDarkRoomEffectのパラメータをシェーダーに送信
        var stack = VolumeManager.instance.stack;
        if (stack == null)
        {
            return;
        }

        var darkRoom = stack.GetComponent<DarkRoomEffect>();
        if (darkRoom == null || !darkRoom.IsActive())
        {
            return;
        }

        float radiusMul = isLightOn ? lightOnRadiusMultiplier : 1f;
        float colorMul = isLightOn ? lightOnColorMultiplier : 1f;

        Color lightColor = darkRoom.lightColor.value;
        lightColor.r *= colorMul;
        lightColor.g *= colorMul;
        lightColor.b *= colorMul;

        Shader.SetGlobalColor("_BaseColor", darkRoom.baseColor.value);
        Shader.SetGlobalColor("_LightColor", lightColor);
        Shader.SetGlobalFloat("_LightRadius", darkRoom.lightRadius.value * radiusMul);
        Shader.SetGlobalFloat("_Softness", darkRoom.softness.value);
        Shader.SetGlobalFloat("_SpotAngle", darkRoom.spotAngle.value);
        Shader.SetGlobalFloat("_ParticleIntensity", darkRoom.particleIntensity.value);
    }
}