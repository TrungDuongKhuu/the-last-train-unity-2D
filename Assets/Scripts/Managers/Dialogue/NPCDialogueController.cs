using UnityEngine;
using System.Collections; // ⭐ Cần thiết cho Coroutine (Độ trễ) ⭐
using System.Collections.Generic;
using HeneGames.DialogueSystem;

public class NPCDialogueController : MonoBehaviour
{
    [Header("CẤU HÌNH CÁC CHỦ ĐỀ ĐỐI THOẠI")]
    [Tooltip("Kéo các GameObject con (chứa CustomMonologue hoặc DialogueManager) từ Hierarchy vào đây.")]
    public List<GameObject> dialogueTopics = new List<GameObject>();

    [Header("TRẠNG THÁI HIỆN TẠI")]
    [Tooltip("Chỉ mục của Topic (Stage) đang hoạt động.")]
    [SerializeField] private int currentTopicIndex = 0;

    [Header("CẤU HÌNH LƯU TRẠNG THÁI")]
    [Tooltip("Có lưu Topic Index của NPC bằng PlayerPrefs không?")]
    [SerializeField] private bool saveTopicIndex = true;                 // ⭐ MỚI

    [Tooltip("Key dùng để lưu PlayerPrefs. Để trống sẽ tự tạo theo tên NPC.")]
    [SerializeField] private string topicIndexPrefsKey = "";             // ⭐ MỚI

    [Header("Cấu hình Kích hoạt")]
    [Tooltip("Độ trễ (giây) trước khi đối thoại tự động chạy sau khi chạm NPC.")]
    [SerializeField] private float activationDelay = 0.5f;

    // References
    private DialogueManager npcDialogueManager;
    private bool playerIsInRange = false;

    private void Awake()
    {
        // ⭐ Thiết lập key nếu chưa nhập tay
        if (saveTopicIndex)
        {
            if (string.IsNullOrEmpty(topicIndexPrefsKey))
            {
                topicIndexPrefsKey = $"{gameObject.name}_TopicIndex";    // Ví dụ: "NamNPC_TopicIndex"
            }

            // ⭐ Load Topic Index đã lưu (nếu có). Mặc định dùng giá trị hiện có trong Inspector.
            currentTopicIndex = PlayerPrefs.GetInt(topicIndexPrefsKey, currentTopicIndex);
            // Debug.Log($"[{name}] Load Topic Index = {currentTopicIndex} (key = {topicIndexPrefsKey})");
        }

        DisableAllTopics();
    }

    // --- LOGIC TRIGGER & KÍCH HOẠT ---

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<DialogueTrigger>()) HandlePlayerEnter();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<DialogueTrigger>()) HandlePlayerEnter();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<DialogueTrigger>()) HandlePlayerExit();
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<DialogueTrigger>()) HandlePlayerExit();
    }

    private void HandlePlayerEnter()
    {
        playerIsInRange = true;

        // ⭐ THÊM LOGIC TRỄ: Bắt đầu Coroutine thay vì gọi ActivateCurrentTopic() ngay ⭐
        StopAllCoroutines();
        StartCoroutine(StartDialogueWithDelay());
    }

    private void HandlePlayerExit()
    {
        playerIsInRange = false;

        // Dừng mọi Coroutine (bao gồm cả Coroutine chờ)
        StopAllCoroutines();

        DisableCurrentTopic();
        if (npcDialogueManager != null) npcDialogueManager.StopDialogue();
    }

    // ⭐ HÀM MỚI: COROUTINE CÓ ĐỘ TRỄ ⭐
    private IEnumerator StartDialogueWithDelay()
    {
        // Đợi khoảng thời gian trễ (ví dụ: 0.5s)
        yield return new WaitForSeconds(activationDelay);

        // Chỉ kích hoạt nếu Player VẪN ĐANG TRONG PHẠM VI sau khi đợi
        if (playerIsInRange)
        {
            ActivateCurrentTopic();
            // Ẩn Interaction UI vì đối thoại đã bắt đầu
            //if (DialogueUI.instance != null) DialogueUI.instance.ShowInteractionUI(false);
        }
    }

    // --- HÀM QUẢN LÝ BẬT/TẮT OBJECT ---

    private void ActivateCurrentTopic()
    {
        if (currentTopicIndex >= 0 && currentTopicIndex < dialogueTopics.Count)
        {
            GameObject currentTopic = dialogueTopics[currentTopicIndex];
            if (currentTopic != null && !currentTopic.activeSelf)
            {
                currentTopic.SetActive(true);
            }
        }
    }

    private void DisableCurrentTopic()
    {
        if (currentTopicIndex >= 0 && currentTopicIndex < dialogueTopics.Count)
        {
            GameObject currentTopic = dialogueTopics[currentTopicIndex];
            if (currentTopic != null && currentTopic.activeSelf)
            {
                currentTopic.SetActive(false);

                // GỌI HÀM RESET
                CustomMonologue customMonologue = currentTopic.GetComponent<CustomMonologue>();
                if (customMonologue != null)
                {
                    customMonologue.ResetMonologue();
                }
            }
        }
    }

    private void DisableAllTopics()
    {
        foreach (var topic in dialogueTopics)
        {
            if (topic != null) topic.SetActive(false);
        }
    }

    // ----------------------------------------------------------------------
    // HÀM PUBLIC ĐỂ UPDATE STAGE
    // ----------------------------------------------------------------------
    public void UpdateTopicIndex(int newIndex)
    {
        if (newIndex >= 0 && newIndex < dialogueTopics.Count)
        {
            // Tắt Object cũ VÀ RESET TRẠNG THÁI CỦA NÓ
            DisableCurrentTopic();

            currentTopicIndex = newIndex;
            Debug.Log($"[{name}] Topic Index updated to: {newIndex} ({dialogueTopics[newIndex].name})");

            // ⭐ LƯU LẠI TOPIC INDEX VÀO PLAYERPREFS
            if (saveTopicIndex)
            {
                PlayerPrefs.SetInt(topicIndexPrefsKey, currentTopicIndex);
                PlayerPrefs.Save();
                // Debug.Log($"[{name}] Saved Topic Index = {currentTopicIndex} (key = {topicIndexPrefsKey})");
            }

            // ⭐ LOGIC KÍCH HOẠT LẠI: BẮT ĐẦU COROUTINE MỚI CHO STAGE MỚI ⭐
            if (playerIsInRange)
            {
                // Dừng mọi coroutine cũ và bắt đầu Coroutine cho Stage mới
                StopAllCoroutines();
                StartCoroutine(StartDialogueWithDelay());

                // Hiển thị lại UI ngay lập tức để người chơi biết Stage đã chuyển
                //if (DialogueUI.instance != null) DialogueUI.instance.ShowInteractionUI(true);
            }
        }
        else
        {
            Debug.LogError($"Index {newIndex} is out of bounds (0 to {dialogueTopics.Count - 1}).");
        }
    }

    // ⭐ Tiện cho debug: reset tiến trình NPC (chuột phải trên component) ⭐
    [ContextMenu("Reset NPC Topic Progress")]
    private void ResetTopicProgress()
    {
        currentTopicIndex = 0;
        if (saveTopicIndex && !string.IsNullOrEmpty(topicIndexPrefsKey))
        {
            PlayerPrefs.DeleteKey(topicIndexPrefsKey);
            PlayerPrefs.Save();
        }
        DisableAllTopics();
        Debug.Log($"[{name}] Topic progress reset.");
    }
}
