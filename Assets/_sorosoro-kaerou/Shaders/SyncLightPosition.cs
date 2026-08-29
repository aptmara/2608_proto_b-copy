using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class SyncLightPosition : MonoBehaviour
{
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

        Shader.SetGlobalColor("_BaseColor", darkRoom.baseColor.value);
        Shader.SetGlobalColor("_LightColor", darkRoom.lightColor.value);
        Shader.SetGlobalFloat("_LightRadius", darkRoom.lightRadius.value);
        Shader.SetGlobalFloat("_Softness", darkRoom.softness.value);
        Shader.SetGlobalFloat("_SpotAngle", darkRoom.spotAngle.value);
        Shader.SetGlobalFloat("_ParticleIntensity", darkRoom.particleIntensity.value);
    }
}