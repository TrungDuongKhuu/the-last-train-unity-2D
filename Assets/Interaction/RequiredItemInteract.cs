using UnityEngine;

/// <summary>
/// Script tổng quát để kiểm tra xem người chơi có vật phẩm yêu cầu trong túi đồ hay không.
/// Gắn script này CÙNG với ConditionalInteractable trên đối tượng tương tác.
/// </summary>
[RequireComponent(typeof(ConditionalInteractable))]
public class RequiredItemInteract : MonoBehaviour
{
    [Header("Item Requirement")]
    [Tooltip("Kéo ItemData của vật phẩm cần thiết vào đây.")]
    [SerializeField] private ItemData requiredItem;

    [Tooltip("Tick vào đây nếu vật phẩm sẽ bị xóa khỏi túi đồ sau khi sử dụng thành công.")]
    [SerializeField] private bool consumeItemOnUse = false;

    private ConditionalInteractable conditionalInteractable;

    private void Awake()
    {
        // Lấy component ConditionalInteractable trên cùng một đối tượng
        conditionalInteractable = GetComponent<ConditionalInteractable>();
    }

    /// <summary>
    /// Hàm này sẽ được gọi TRƯỚC khi hàm Interact() mặc định được gọi.
    /// Nó kiểm tra túi đồ và cập nhật điều kiện cho ConditionalInteractable.
    /// </summary>
    public void CheckInventoryForItem()
    {
        if (requiredItem == null)
        {
            Debug.LogError("Chưa gán 'Required Item' trong RequiredItemInteract!", this);
            return;
        }

        // Kiểm tra xem trong túi đồ có vật phẩm yêu cầu không
        bool hasItem = InventoryManager.instance.HasItem(requiredItem);

        // Cập nhật điều kiện
        conditionalInteractable.isConditionMet = hasItem;

        // Nếu có vật phẩm và cần tiêu thụ, thì xóa nó đi SAU KHI tương tác thành công
        if (hasItem && consumeItemOnUse)
        {
            // Chúng ta sẽ xóa vật phẩm trong sự kiện OnSuccess của ConditionalInteractable
            // để đảm bảo nó chỉ bị xóa khi hành động thực sự xảy ra.
            Debug.Log($"Vật phẩm '{requiredItem.itemName}' sẽ bị tiêu thụ sau khi sử dụng.");
        }
    }

    /// <summary>
    /// Hàm này được gọi từ sự kiện OnSuccess của ConditionalInteractable để xóa vật phẩm.
    /// </summary>
    public void ConsumeRequiredItem()
    {
        if (requiredItem != null && consumeItemOnUse)
        {
            InventoryManager.instance.Remove(requiredItem);
            Debug.Log($"Đã tiêu thụ vật phẩm: '{requiredItem.itemName}'.");
        }
    }
}

