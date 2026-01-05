using UnityEngine;

/// <summary>
/// Kế thừa từ ItemPickup nhưng thay đổi hành vi tương tác.
/// Khi người chơi nhặt vật phẩm này, nó sẽ được thêm vào túi đồ
/// nhưng đối tượng gốc sẽ KHÔNG bị phá hủy.
/// </summary>
public class PermanentItemPickup : ItemPickup
{
    /// <summary>
    /// Ghi đè (override) phương thức Interact của lớp cha (ItemPickup).
    /// </summary>
    public override void Interact()
    {
        // Gọi hàm để thêm vật phẩm vào túi đồ
        bool wasPickedUp = InventoryManager.instance.Add(item);

        // Nếu vật phẩm được nhặt thành công (túi đồ chưa đầy)
        if (wasPickedUp)
        {
            Debug.Log($"Đã nhặt vật phẩm (không phá hủy): {item.itemName}");
            // Vô hiệu hóa đối tượng để nó biến mất khỏi màn hình
        }
    }
}
