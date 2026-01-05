using UnityEngine;

/// <summary>
/// Lớp cha cho các vật phẩm có thể sử dụng nhiều lần mà không bị mất đi.
/// Ví dụ: Chìa khóa, đèn pin, các vật phẩm nhiệm vụ...
/// Lớp này vẫn là abstract, buộc các lớp con phải định nghĩa hành vi "Use" cụ thể.
/// </summary>

[CreateAssetMenu(fileName = "New Permanent Item Data", menuName = "Inventory/Permanent Item")]
public class PermanentItemData : ItemData
{
    /// <summary>
    /// Ghi đè phương thức Use.
    /// Vật phẩm vĩnh viễn không bị xóa sau khi sử dụng,
    /// vì vậy phương thức này có thể để trống hoặc có hành vi tùy chỉnh.
    /// </summary>
    public override void Use(Controller playerController)
    {
        // Vật phẩm vĩnh viễn không bị xóa sau khi sử dụng.
        // Hành vi cụ thể sẽ được định nghĩa trong các lớp con.
        Debug.Log($"Đã sử dụng vật phẩm vĩnh viễn: {itemName}, nhưng nó vẫn còn trong túi đồ.");
    }
}
