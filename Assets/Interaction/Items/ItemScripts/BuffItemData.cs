using UnityEngine;

[CreateAssetMenu(fileName = "New Buff Item", menuName = "Inventory/Buff Item")]
public class BuffItemData : ItemData
{
    [Header("Buff to Apply")]
    public BuffEffect buff;

    public override void Use(Controller playerController)
    {
        // Gọi phương thức trên Controller để thêm hiệu ứng
        playerController.ApplyBuff(buff);
        Debug.Log($"Used {itemName}, applied buff: {buff.effectName}");
        // Xóa vật phẩm khỏi túi đồ sau khi sử dụng
        InventoryManager.instance.Remove(this);
    }
}
