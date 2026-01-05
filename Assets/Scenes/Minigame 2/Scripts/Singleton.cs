using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Component
{
    private static T s_instance;

    public static T Instance
    {
        get
        {
            if (s_instance == null)
            {
                s_instance = FindFirstObjectByType<T>();
                if (s_instance == null)
                    Debug.LogError($"No instance of {typeof(T).Name} found.");
            }
            return s_instance;
        }
    }

    protected virtual void Awake()
    {
        if (s_instance == null)
        {
            s_instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (s_instance != this)
        {
            Destroy(gameObject);
        }
    }
}
