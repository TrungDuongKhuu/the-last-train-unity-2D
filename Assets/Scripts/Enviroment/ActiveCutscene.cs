using UnityEngine;
using UnityEngine.Playables;

public class ActiveCutscene : MonoBehaviour
{
    [Header("Thiết lập Cutscene")]
    public PlayableDirector cutsceneDirector;

    [Header("Tuỳ chọn")]
    public bool triggerOnce = true;

    [Header("PlayerPrefs Key (đặt riêng cho từng cutscene)")]
    public string cutsceneKey = "CUTSCENE_DEFAULT";

    private bool hasTriggered = false;
    private Collider2D col;

    private void Start()
    {
        col = GetComponent<Collider2D>();

        // Load trạng thái từ PlayerPrefs
        hasTriggered = PlayerPrefs.GetInt(cutsceneKey, 0) == 1;

        // Nếu đã chạy và chỉ chạy 1 lần → disable trigger
        if (triggerOnce && hasTriggered && col != null)
        {
            col.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        PlayCutscene(); // 🔥 dùng chung – không gọi trực tiếp Play()
    }

    /// <summary>
    /// Hàm GLOBAL PLAY — gọi từ bất kỳ đâu cũng chạy logic kiểm tra.
    /// </summary>
    public void PlayCutscene()
    {
        // Nếu đã chạy và chỉ trigger 1 lần → không chạy lại
        if (hasTriggered && triggerOnce) return;

        if (cutsceneDirector == null)
        {
            Debug.LogWarning("[ActiveCutscene] Missing PlayableDirector!");
            return;
        }

        // Play cutscene
        cutsceneDirector.Play();

        // Lưu trạng thái
        hasTriggered = true;
        PlayerPrefs.SetInt(cutsceneKey, 1);
        PlayerPrefs.Save();
        PlayerPrefsMonitor.NotifyChanged(cutsceneKey);

        // Disable trigger nếu cần
        if (triggerOnce && col != null)
        {
            col.enabled = false;
        }
    }
}
