using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InventoryManager : MonoBehaviour
{
    #region Singleton
    public static InventoryManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            
            // Chỉ gọi DontDestroyOnLoad nếu GameObject chưa nằm trong DontDestroyOnLoad scene
            if (gameObject.scene.name != "DontDestroyOnLoad")
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            // Giữ lại cảnh báo này để phát hiện duplicate trong tương lai
            Debug.LogError($"Duplicate InventoryManager found on GameObject: {gameObject.name}. Destroying it.");
            Destroy(gameObject);
            return; // Dừng ngay để tránh xử lý thêm
        }
    }
    #endregion

    public List<ItemData> items = new List<ItemData>();
    public int space = 4;

    public int currentSelectedSlot = 0;

    // TÁCH THÀNH 2 SỰ KIỆN RIÊNG BIỆT
    // 1. Chỉ gọi khi thêm hoặc xóa vật phẩm
    public UnityEvent onInventoryChanged; 
    // 2. Chỉ gọi khi thay đổi ô lựa chọn
    public UnityEvent onSelectionChanged; 

    private void Start()
    {
        // Kích hoạt cả hai event lúc đầu để UI hiển thị đúng
        onInventoryChanged.Invoke();
        onSelectionChanged.Invoke();
    }

    public bool Add(ItemData item)
    {
        if (items.Count >= space)
        {
            Debug.Log("Not enough room in inventory.");
            return false;
        }

        if (item == null)
        {
            Debug.LogError("Trying to add NULL item to inventory!");
            return false;
        }

        bool wasEmpty = items.Count == 0;
        items.Add(item);

        // Nếu đây là vật phẩm đầu tiên, reset selection về 0
        if (wasEmpty)
        {
            currentSelectedSlot = 0;
            
            // Kích hoạt cả hai event
            if (onInventoryChanged != null)
                onInventoryChanged.Invoke();
            if (onSelectionChanged != null)
                onSelectionChanged.Invoke();
        }
        else
        {
            // Chỉ kích hoạt sự kiện thay đổi túi đồ
            if (onInventoryChanged != null)
                onInventoryChanged.Invoke();
        }
        
        Debug.Log($"{item.itemName} was added. Total items: {items.Count}");
        return true;
    }

    public void Remove(ItemData item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
            Debug.Log($"{item.itemName} was removed. Total items: {items.Count}");

            if (onInventoryChanged != null)
            {
                onInventoryChanged.Invoke();
            }
        }
        else
        {
            Debug.LogWarning($"Item not found in inventory: {(item != null ? item.itemName : "NULL")}");
        }
    }

    public void RemoveAt(int slotIndex)
    {
        if (slotIndex < items.Count && slotIndex >= 0)
        {
            string itemName = items[slotIndex].itemName;
            items.RemoveAt(slotIndex);
            Debug.Log($"Used {itemName} from slot {slotIndex}. Total items: {items.Count}");

            // Cập nhật currentSelectedSlot để tránh index out of bounds
            if (items.Count == 0)
            {
                currentSelectedSlot = 0;
            }
            else if (currentSelectedSlot >= items.Count)
            {
                currentSelectedSlot = items.Count - 1;
            }

            if (onInventoryChanged != null)
            {
                onInventoryChanged.Invoke();
            }

            // Cập nhật selection UI vì currentSelectedSlot có thể đã thay đổi
            if (onSelectionChanged != null)
            {
                onSelectionChanged.Invoke();
            }
        }
        else
        {
            Debug.LogWarning($"Cannot remove item: invalid slot index {slotIndex}");
        }
    }

    public ItemData GetSelectedItem()
    {
        if (items.Count == 0)
        {
            currentSelectedSlot = 0;
            return null;
        }

        // Tự động điều chỉnh currentSelectedSlot nếu nó không hợp lệ
        if (currentSelectedSlot >= items.Count)
        {
            currentSelectedSlot = items.Count - 1;
            Debug.Log($"Auto-adjusted currentSelectedSlot to {currentSelectedSlot}");
            
            // Thông báo UI cập nhật vị trí selection
            if (onSelectionChanged != null)
            {
                onSelectionChanged.Invoke();
            }
        }
        else if (currentSelectedSlot < 0)
        {
            currentSelectedSlot = 0;
            Debug.Log($"Auto-adjusted currentSelectedSlot to {currentSelectedSlot}");
            
            if (onSelectionChanged != null)
            {
                onSelectionChanged.Invoke();
            }
        }

        return items[currentSelectedSlot];
    }

    public void ChangeSelectedItem(int direction)
{
    if (items.Count == 0) 
    {
        currentSelectedSlot = 0;
        return;
    }

    // Cập nhật vị trí
    currentSelectedSlot += direction;

    // Logic vòng lặp mới, an toàn và chính xác
    if (currentSelectedSlot < 0)
    {
        // Nếu đi lùi từ vị trí 0, nhảy đến vị trí cuối cùng
        currentSelectedSlot = items.Count - 1;
    }
    else
    {
        // Sử dụng toán tử modulo để tự động quay về 0 khi vượt quá giới hạn
        currentSelectedSlot %= items.Count;
    }

    // Chỉ kích hoạt sự kiện thay đổi lựa chọn
    if (onSelectionChanged != null)
    {
        onSelectionChanged.Invoke();
    }
    Debug.Log("Selected slot: " + currentSelectedSlot);
}

    /// <summary>
    /// Kiểm tra xem một vật phẩm cụ thể có trong túi đồ hay không.
    /// </summary>
    /// <param name="itemToCheck">ItemData của vật phẩm cần kiểm tra.</param>
    /// <returns>True nếu có, False nếu không.</returns>
    public bool HasItem(ItemData itemToCheck)
    {
        if (itemToCheck == null) return false;
        return items.Contains(itemToCheck);
    }

    /// <summary>
    /// Vứt vật phẩm đang được chọn ra khỏi túi đồ và xóa nó vĩnh viễn.
    /// </summary>
    public void DropSelectedItem()
    {
        // Kiểm tra xem có vật phẩm nào để vứt không
        if (items.Count > 0 && currentSelectedSlot >= 0 && currentSelectedSlot < items.Count)
        {
            ItemData itemToDrop = items[currentSelectedSlot];
            
            Debug.Log($"Vứt bỏ vật phẩm: {itemToDrop.itemName}");

            // Chỉ cần xóa vật phẩm khỏi túi đồ
            Remove(itemToDrop);
        }
    }
}