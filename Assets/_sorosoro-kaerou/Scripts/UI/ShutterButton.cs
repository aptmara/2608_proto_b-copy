using UnityEngine;
using UnityEngine.UI;

// AndroidManagerはSingleton<AndroidManager>で、破棄・再生成されうる。
// Inspector上でOnClickに直接AndroidTorchをドラッグ&ドロップすると参照切れの恐れがあるため、
// コードからAndroidManager.Instanceを都度解決してから呼び出す。
public class ShutterButton : MonoBehaviour
{
    [SerializeField] private Button button;

    private void Start()
    {
        button.onClick.AddListener(OnClickShutter);
    }

    private void OnClickShutter()
    {
        AndroidManager.Instance?.Torch?.FlashAndSpawn();
    }
}
