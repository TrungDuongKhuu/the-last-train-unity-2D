using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeneGames.DialogueSystem; // Cần namespace này để sử dụng DialogueCharacter và NPC_Sentence

public class CustomMonologue : MonoBehaviour // Tên class mới
{
    [Header("Monologue Configuration")]
    [Tooltip("Delay in seconds before the monologue starts after the scene loads.")]
    [SerializeField] private float startDelay = 0.5f;

    [Header("Monologue Content")]
    // [SerializeField] private List<NPC_Sentence> monologueSentences; // Biến gốc

    // ⭐ SỬA ĐỔI: Khôi phục và giữ nguyên tên biến gốc
    [SerializeField] private List<NPC_Sentence> monologueSentences;

    // ⭐ SỬA ĐỔI 1: Đổi từ private thành public để Controller có thể truy cập và reset trực tiếp ⭐
    public bool hasTriggered = false;

    // Hàm OnEnable() sẽ thay thế hàm Start() cũ để cho phép script chạy lại khi Object được bật
    void OnEnable()
    {
        // Gợi ý: Nếu bạn muốn nó chỉ chạy khi được gọi bởi Controller chứ không phải khi Scene Load,
        // bạn có thể thêm một cờ phụ vào đây. Nhưng dựa trên logic hiện tại, ta để nó tự chạy.

        // Bắt đầu coroutine để kích hoạt độc thoại sau một khoảng trễ
        if (!hasTriggered && monologueSentences != null && monologueSentences.Count > 0)
        {
            // Bắt đầu Coroutine để xử lý trễ và logic tìm kiếm Player
            StartCoroutine(StartMonologueAfterDelay());
        }
        else if (hasTriggered)
        {
            // Nếu đã chạy rồi, và Object được bật lại, nó sẽ log ra thông báo này
            // Logic này sẽ được reset bằng hàm ResetMonologue()
            Debug.Log($"CustomMonologue trên {gameObject.name} đã chạy và cần reset.");
        }
    }

    // ⭐ HÀM Start() được giữ trống hoặc xóa bỏ vì logic đã chuyển sang OnEnable() ⭐
    void Start() { /* KHÔNG CÓ LOGIC GÌ Ở ĐÂY */ }

    // Hàm OnDisable() để dừng Coroutine khi Object bị tắt
    void OnDisable()
    {
        StopAllCoroutines();
    }


    private IEnumerator StartMonologueAfterDelay()
    {
        // ... (Giữ nguyên toàn bộ nội dung Coroutine cũ) ...
        yield return new WaitForSeconds(startDelay);

        // --- Logic Tìm kiếm Player (Giữ nguyên từ script gốc) ---
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("CustomMonologue Error: Cannot find GameObject with tag 'Player'. Make sure your player object has the correct tag.");
            yield break;
        }

        DialogueManager playerDialogueManager = player.GetComponent<DialogueManager>();

        if (playerDialogueManager == null)
        {
            Debug.LogError("CustomMonologue Error: The 'Player' GameObject does not have a DialogueManager component attached.");
            yield break;
        }

        // Đánh dấu là đã kích hoạt để không chạy lại trừ khi được reset
        // Đây là điểm mà NPCDialogueController sẽ phải reset bằng cách gọi ResetMonologue()
        hasTriggered = true;

        // Bắt đầu đoạn độc thoại bằng cách gọi hàm công khai
        playerDialogueManager.StartCustomDialogue(monologueSentences);
    }

    // ⭐ HÀM FIX LỖI: Thêm hàm công khai để Controller reset cờ (Được gọi bởi NPCDialogueController.DisableCurrentTopic) ⭐
    /// <summary>
    /// Được gọi bởi Controller để reset cờ trạng thái, cho phép Monologue chạy lại.
    /// </summary>
    public void ResetMonologue()
    {
        hasTriggered = false;
        Debug.Log($"Monologue trên {gameObject.name} đã được reset.");
    }
}