using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform itemsParent;
    // selectionBorder không cần nữa vì dùng màu border của slot

    private InventoryManager inventory;
    private InventorySlot[] slots;
    public static InventoryUI Instance;
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Kiểm tra null để tránh crash
        if (InventoryManager.instance == null)
        {
            Debug.LogError("InventoryManager.instance is null! Make sure there's an InventoryManager in the scene.", this);
            return;
        }

        inventory = InventoryManager.instance;

        // Đăng ký lắng nghe cho cả hai sự kiện
        inventory.onInventoryChanged.AddListener(UpdateInventoryUI);
        inventory.onSelectionChanged.AddListener(UpdateSelectionUI);
        
        Debug.Log("InventoryUI: Event listeners registered successfully!");

        slots = itemsParent.GetComponentsInChildren<InventorySlot>();
        Debug.Log("Inventory slots found: " + slots.Length);
        
        // Kiểm tra xem có tìm thấy slots không
        if (slots.Length == 0)
        {
            Debug.LogError("No InventorySlot components found! Check itemsParent reference.", this);
            return;
        }
        
        UpdateInventoryUI();
        UpdateSelectionUI();
    }

    // Hàm này CHỈ cập nhật icon vật phẩm
    void UpdateInventoryUI()
    {
        // Kiểm tra an toàn trước khi cập nhật
        if (inventory == null || slots == null || slots.Length == 0)
        {
            Debug.LogWarning("Cannot update inventory UI: missing references", this);
            return;
        }

        Debug.Log($"Updating UI: {inventory.items.Count} items, {slots.Length} slots");

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inventory.items.Count)
            {
                Debug.Log($"Slot {i}: Adding {inventory.items[i].itemName}");
                slots[i].AddItem(inventory.items[i]);
            }
            else
            {
                slots[i].ClearSlot(); // ClearSlot() sẽ tự động đặt màu xám mặc định
            }
        }
        
        // Sau khi cập nhật xong icon, gọi UpdateSelectionUI để đảm bảo màu sắc đúng
        UpdateSelectionUI();
    }

    // Hàm này CHỈ cập nhật màu border
    void UpdateSelectionUI()
    {
        Debug.Log("UpdateSelectionUI() CALLED!");
        
        // Chỉ cần kiểm tra inventory và slots
        if (inventory == null || slots == null || slots.Length == 0)
        {
            Debug.LogError($"UpdateSelectionUI() EARLY RETURN - inventory: {inventory != null}, slots: {slots != null}, slots.Length: {slots?.Length ?? 0}");
            return;
        }

        Debug.Log($"UpdateSelectionUI called - currentSelectedSlot: {inventory.currentSelectedSlot}, items count: {inventory.items.Count}");

        // Cập nhật màu border cho tất cả các slot
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inventory.items.Count)
            {
                // Slot có vật phẩm - đổi màu border
                bool isSelected = (i == inventory.currentSelectedSlot);
                Debug.Log($"Slot {i}: has item, isSelected = {isSelected}");
                slots[i].SetBorderColor(isSelected);
            }
            // Slot trống sẽ tự động có màu xám mặc định từ ClearSlot()
        }
    }

    public void RefreshUI()
    {
        if (inventory == null)
            inventory = InventoryManager.instance;

        if (slots == null || slots.Length == 0)
            slots = GetComponentsInChildren<InventorySlot>(true);

        UpdateInventoryUI();
        UpdateSelectionUI();
    }

}