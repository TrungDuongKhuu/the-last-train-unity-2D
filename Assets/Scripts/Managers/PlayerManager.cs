using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Info")]
    [Tooltip("Đánh dấu đây là Player chính (MC)")]
    public bool isMainPlayer = true;

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterPlayer(this);
    }

    public Camera GetCamera()
    {
        return GetComponentInChildren<Camera>();
    }
}
