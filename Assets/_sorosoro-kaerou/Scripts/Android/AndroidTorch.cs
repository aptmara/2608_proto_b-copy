using SoroSoro.Events;
using UnityEngine;
using UnityEngine.InputSystem;

public class AndroidTorch : MonoBehaviour
{
    /// <summary>現在トーチがONかどうか。</summary>
    public bool IsOn { get; private set; }

    /// <summary>撮影演出実行中かどうか。</summary>
    public bool IsFlashing { get; private set; }

    // InstantiateするPrefab
    public GameObject spawnPrefab;

#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject cameraManager;
    private string cameraId;
#endif

    private void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                cameraManager = activity.Call<AndroidJavaObject>("getSystemService", "camera");
                string[] idList = cameraManager.Call<string[]>("getCameraIdList");

                if (idList != null && idList.Length > 0)
                {
                    cameraId = idList[0];
                }
                else
                {
                    Debug.LogWarning("[TorchController] カメラIDが取得できませんでした。");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TorchController] CameraManagerの初期化に失敗しました: {e}");
        }
#endif
    }

    public void Toggle()
    {
        SetTorch(!IsOn);
    }

    public void SetTorch(bool on)
    {
        bool previous = IsOn;

#if UNITY_ANDROID && !UNITY_EDITOR
        if (cameraManager == null || cameraId == null)
        {
            Debug.LogWarning("[TorchController] CameraManager/カメラIDが未初期化のためトーチを操作できません。");
            return;
        }

        try
        {
            cameraManager.Call("setTorchMode", cameraId, on);
            IsOn = on;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TorchController] setTorchModeの呼び出しに失敗しました: {e}");
            return;
        }
#else
        IsOn = on;
        Debug.Log($"[TorchController] (Editor/非Android) トーチ状態をシミュレート: {on}");
#endif

        if (IsOn != previous)
        {
            GameEvents.RaiseLightOnChanged(IsOn);
        }
    }

    /// <summary>
    /// 点灯・消灯・オブジェクト生成の一連の演出。
    /// </summary>
    public void FlashAndSpawn()
    {
        if (IsFlashing) return;
        StartCoroutine(FlashAndSpawnCoroutine());
    }

    private System.Collections.IEnumerator FlashAndSpawnCoroutine()
    {
        IsFlashing = true;

        // 0.7秒点灯
        SetTorch(true);
        yield return new WaitForSeconds(0.7f);

        // 0.1秒消灯
        SetTorch(false);
        yield return new WaitForSeconds(0.1f);

        // 0.1秒点灯
        SetTorch(true);

        // 点灯と同時にInstantiate
        if (spawnPrefab != null)
        {
            GameObject obj = Instantiate(
                spawnPrefab,
                transform.position,
                transform.rotation
            );

            GameEvents.RaiseShutterRequested();

            // 3秒後にこのオブジェクトを削除
            Destroy(obj, 3f);
        }
        else
        {
            Debug.LogWarning("[TorchController] spawnPrefabが設定されていません。");
        }

        yield return new WaitForSeconds(0.1f);

        // 最後にトーチを消す
        SetTorch(false);

        IsFlashing = false;
    }
}