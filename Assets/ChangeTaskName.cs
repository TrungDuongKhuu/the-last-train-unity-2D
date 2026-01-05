using TMPro;
using UnityEngine;

public class ChangeTaskName : MonoBehaviour
{
    public TextMeshProUGUI taskNameText;

    public void ChangeName(string newName)
    {
        // --- Bắt đầu gỡ lỗi ---

        // 1. Kiểm tra xem taskNameText đã được gán trong Inspector chưa
        if (taskNameText == null)
        {
            Debug.LogError("LỖI: Biến 'taskNameText' chưa được gán trong Inspector!", this.gameObject);
            return; // Dừng hàm tại đây để tránh lỗi
        }

        // 2. In ra thông báo khi hàm được gọi và tên mới là gì
        Debug.Log($"Hàm ChangeName được gọi với tên mới: '{newName}'", this.gameObject);

        // --- Kết thúc gỡ lỗi ---

        taskNameText.text = newName;
    }
}
