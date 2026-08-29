using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    private AndroidJavaObject cameraManager;
    private string cameraId;

    private bool isLightOn = false;


    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        using (AndroidJavaClass unityPlayer =
            new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject activity =
                unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            cameraManager =
                activity.Call<AndroidJavaObject>(
                    "getSystemService",
                    "camera"
                );

            // 使用するカメラを探す
            using (AndroidJavaClass cameraManagerClass =
                new AndroidJavaClass("android.hardware.camera2.CameraManager"))
            {
                string[] cameraIds =
                    cameraManager.Call<string[]>("getCameraIdList");

                foreach (string id in cameraIds)
                {
                    AndroidJavaObject characteristics =
                        cameraManager.Call<AndroidJavaObject>(
                            "getCameraCharacteristics",
                            id
                        );

                    using (AndroidJavaClass characteristicsClass =
                        new AndroidJavaClass(
                            "android.hardware.camera2.CameraCharacteristics"))
                    {
                        int lensFacingKey =
                            characteristicsClass.GetStatic<int>(
                                "LENS_FACING"
                            );

                        int lensFacing =
                            characteristics.Call<int>(
                                "get",
                                lensFacingKey
                            );

                        // 背面カメラ
                        if (lensFacing == 1)
                        {
                            cameraId = id;
                            break;
                        }
                    }
                }
            }
        }

        Debug.Log("Flashlight Camera ID: " + cameraId);

#endif
    }


    // =========================================
    // ライト ON
    // =========================================

    public void LightOn()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (cameraManager == null || string.IsNullOrEmpty(cameraId))
        {
            Debug.LogError("CameraManagerまたはCamera IDが取得できていません。");
            return;
        }

        try
        {
            cameraManager.Call(
                "setTorchMode",
                cameraId,
                true
            );

            isLightOn = true;

            Debug.Log("ライト ON");
        }
        catch (AndroidJavaException e)
        {
            Debug.LogError("ライトON失敗: " + e.Message);
        }

#endif
    }


    // =========================================
    // ライト OFF
    // =========================================

    public void LightOff()
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        if (cameraManager == null || string.IsNullOrEmpty(cameraId))
        {
            return;
        }

        try
        {
            cameraManager.Call(
                "setTorchMode",
                cameraId,
                false
            );

            isLightOn = false;

            Debug.Log("ライト OFF");
        }
        catch (AndroidJavaException e)
        {
            Debug.LogError("ライトOFF失敗: " + e.Message);
        }

#endif
    }


    // =========================================
    // ON / OFF 切り替え
    // =========================================

    public void ToggleLight()
    {
        if (isLightOn)
        {
            LightOff();
        }
        else
        {
            LightOn();
        }
    }
}