using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();

    // 【追加】アプリ終了中かどうかを判定するフラグ
    private static bool _isQuitting = false;

    // インスタンスが存在するかどうかをチェックする用（生成をトリガーしない）
    public static bool HasInstance => _instance != null;

    public static T Instance
    {
        get
        {
            if (_isQuitting)
            {
                // Debug.LogWarning($"[Singleton] {typeof(T).Name} はアプリ終了中のため、アクセスを拒否しました。");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();

                    if (_instance == null)
                    {
                        var prefab = Resources.Load<T>("Manager/" + typeof(T).Name);
                        if (prefab != null)
                        {
                            _instance = Instantiate(prefab);
                        }
                        else
                        {
                            if (Application.isPlaying)
                            {
                                Debug.LogError(typeof(T).Name + " のプレハブが見つかりません。");
                            }
                        }
                    }
                }
                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this as T)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this as T;
        if (transform.parent == null)
        {
            DontDestroyOnLoad(this.gameObject);
        }
        _isQuitting = false; // 再生開始時にリセット
    }

    protected virtual void OnApplicationQuit()
    {
        _isQuitting = true;
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this as T)
        {
            _instance = null;
        }
    }
}
