using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// Lớp chứa thông tin về một vật phẩm được quản lý
[System.Serializable]
public class ManagedItem
{
    [Tooltip("Tên định danh duy nhất cho vật phẩm, ví dụ: 'Chìa khóa phòng 101', 'Cần gạt tầng hầm'.")]
    public string itemId;

    [Tooltip("Kéo GameObject của vật phẩm vào đây.")]
    public GameObject itemObject;

    [Tooltip("Trạng thái ban đầu của vật phẩm khi vào Scene (hiện hay ẩn).")]
    public bool initiallyActive = false;
}

/// <summary>
/// Quản lý trạng thái (hiện/ẩn) của các vật phẩm quan trọng trong một Scene.
/// Hoạt động như một Singleton để dễ dàng truy cập từ các script khác (TaskManager, DialogueManager...).
/// </summary>
public class SceneItemManager : MonoBehaviour
{
    // Singleton instance để truy cập toàn cục
    public static SceneItemManager instance;

    [Header("Danh sách các vật phẩm được quản lý")]
    [SerializeField] private List<ManagedItem> managedItems = new List<ManagedItem>();

    // Dictionary để truy cập nhanh, tối ưu hơn là duyệt list mỗi lần
    private Dictionary<string, GameObject> itemDictionary = new Dictionary<string, GameObject>();

    private void Awake()
    {
        // --- Singleton Pattern ---
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("Đã có một SceneItemManager khác trong Scene. Hủy bỏ đối tượng này.", this.gameObject);
            Destroy(gameObject);
            return;
        }
        // --- End Singleton ---

        // Chuyển List từ Inspector vào Dictionary để truy cập nhanh hơn
        foreach (var item in managedItems)
        {
            if (item.itemObject == null)
            {
                Debug.LogError($"Vật phẩm với ID '{item.itemId}' chưa được gán GameObject trong Inspector!", this);
                continue;
            }
            if (!itemDictionary.ContainsKey(item.itemId))
            {
                itemDictionary.Add(item.itemId, item.itemObject);
            }
            else
            {
                Debug.LogWarning($"ID vật phẩm bị trùng lặp: '{item.itemId}'. Vui lòng sử dụng ID duy nhất.", this);
            }
        }
    }

    private void Start()
    {
        // Áp dụng trạng thái ban đầu cho tất cả các vật phẩm
        ApplyInitialStates();
    }

    /// <summary>
    /// Thiết lập trạng thái ban đầu (hiện/ẩn) cho tất cả các vật phẩm trong danh sách.
    /// </summary>
    private void ApplyInitialStates()
    {
        foreach (var item in managedItems)
        {
            if (item.itemObject != null)
            {
                item.itemObject.SetActive(item.initiallyActive);
            }
        }
    }

    /// <summary>
    /// Thay đổi trạng thái của một vật phẩm (hiện hoặc ẩn).
    /// </summary>
    /// <param name="itemId">ID của vật phẩm cần thay đổi.</param>
    /// <param name="isActive">True để hiện, False để ẩn.</param>
    public void SetItemState(string itemId, bool isActive)
    {
        if (itemDictionary.TryGetValue(itemId, out GameObject itemObject))
        {
            itemObject.SetActive(isActive);
            Debug.Log($"Đã thay đổi trạng thái vật phẩm '{itemId}' thành '{ (isActive ? "Hiện" : "Ẩn") }'.");
        }
        else
        {
            Debug.LogError($"Không tìm thấy vật phẩm nào với ID: '{itemId}'. Hãy kiểm tra lại ID trong SceneItemManager.");
        }
    }

    /// <summary>
    /// Hàm công khai để kích hoạt một vật phẩm (làm nó hiện ra).
    /// </summary>
    public void ActivateItem(string itemId)
    {
        SetItemState(itemId, true);
    }

    /// <summary>
    /// Hàm công khai để vô hiệu hóa một vật phẩm (làm nó ẩn đi).
    /// </summary>
    public void DeactivateItem(string itemId)
    {
        SetItemState(itemId, false);
    }
}
