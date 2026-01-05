using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneChoiceData : MonoBehaviour
{
    [Header("Data Lựa Chọn Cho Scene Này")]
    [Tooltip("Điền tất cả các nhóm lựa chọn (ChoiceGroup) cho scene này vào đây")]
    public List<DialogueChoiceManager.ChoiceGroup> choicesForThisScene;

    /// <summary>
    /// ĐÃ SỬA: Chuyển từ Awake() sang Start().
    /// Giải thích: 
    /// - Awake: Dùng để khởi tạo CHÍNH BẢN THÂN script này.
    /// - Start: Dùng để giao tiếp với CÁC SCRIPT KHÁC (như Manager).
    /// Khi Start chạy, Unity đảm bảo toàn bộ Awake của mọi object đã xong => Instance chắc chắn tồn tại.
    /// </summary>
    void Start()
    {
        if (DialogueChoiceManager.Instance != null)
        {
            // Nạp data vào Manager
            DialogueChoiceManager.Instance.LoadChoicesForScene(choicesForThisScene);
            Debug.Log($"[SceneChoiceData] Đã nạp {choicesForThisScene.Count} groups vào Manager.");
        }
        else
        {
            // Nếu vẫn null ở Start, nghĩa là Manager chưa bao giờ được tạo hoặc đã bị hủy.
            Debug.LogError("[SceneChoiceData] Vẫn không tìm thấy DialogueChoiceManager! " +
                           "Kiểm tra xem scene '_system' có thực sự đang chạy và chứa Manager không.");
        }
    }
}