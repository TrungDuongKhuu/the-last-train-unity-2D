using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
{
    public ItemData item; // Kéo ItemData asset vào đây
    [Header("Indicator Settings")]
    [SerializeField] protected GameObject indicatorPrefab; // Kéo prefab Indicator vào đây
    [SerializeField] protected Vector3 indicatorOffset = new Vector3(0, 1.2f, 0); // Vị trí của indicator so với vật phẩm

    protected GameObject indicatorInstance;
    public virtual void Interact()
    {
        // Kiểm tra các điều kiện cần thiết
        if (item == null)
        {
            Debug.LogError("ItemPickup: item is null! Assign an ItemData asset.", this);
            return;
        }

        if (InventoryManager.instance == null)
        {
            Debug.LogError("ItemPickup: InventoryManager.instance is null!", this);
            return;
        }

        Debug.Log("Interacting with " + item.itemName);
        bool wasPickedUp = InventoryManager.instance.Add(item);

        if (wasPickedUp)
        {
            Debug.Log($"Successfully picked up {item.itemName}");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log($"Failed to pick up {item.itemName} - inventory might be full");
        }
    }

    public string GetInteractionText()
    {
        if (item != null)
        {
            return $"Nhặt {item.itemName}";
        }
        return "Nhặt vật phẩm";
    }

    // Thêm hai hàm mới để quản lý indicator
    public void ShowIndicator()
    {
        if (indicatorPrefab != null && indicatorInstance == null)
        {
            // Tìm Canvas trong scene
            Canvas mainCanvas = Object.FindFirstObjectByType<Canvas>();
            if (mainCanvas == null)
            {
                Debug.LogError("No Canvas found in the scene for the indicator.");
                return;
            }
            
            // Tạo indicator và đặt nó làm con của Canvas
            indicatorInstance = Instantiate(indicatorPrefab, mainCanvas.transform);
        }

        if (indicatorInstance != null)
        {
            indicatorInstance.SetActive(true);
            // Cập nhật vị trí của indicator theo vị trí của vật phẩm trên màn hình
            indicatorInstance.transform.position = Camera.main.WorldToScreenPoint(transform.position + indicatorOffset);
        }
    }

    public void HideIndicator()
    {
        if (indicatorInstance != null)
        {
            indicatorInstance.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // Rất quan trọng: Phá hủy indicator khi vật phẩm bị phá hủy
        if (indicatorInstance != null)
        {
            Destroy(indicatorInstance);
        }
    }

    private void Update()
    {
        // Cập nhật vị trí indicator mỗi frame nếu nó đang được hiển thị
        if (indicatorInstance != null && indicatorInstance.activeSelf)
        {
            indicatorInstance.transform.position = Camera.main.WorldToScreenPoint(transform.position + indicatorOffset);
        }
    }
}