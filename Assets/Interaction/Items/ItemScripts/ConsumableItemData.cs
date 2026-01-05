using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory/Consumable Item")]
public class ConsumableItemData : ItemData
{
    [Header("Consumable Effect")]
    public float healthToRestore = 0f;
    public float manaToRestore = 0f;

    public override void Use(Controller playerController)
    {
        bool wasUsed = false;
        
        // Hồi máu
        if (healthToRestore > 0)
        {
            // Chỉ hồi máu nếu máu chưa đầy
            if (playerController.RestoreHealth(healthToRestore))
            {
                Debug.Log($"Used {itemName} to restore {healthToRestore} HP.");
                wasUsed = true;
            }
            else
            {
                Debug.Log("Máu đã đầy, không thể dùng vật phẩm hồi máu.");
            }
        }

        // Hồi mana (tương tự, bạn có thể thêm hàm RestoreMana trả về bool nếu muốn)
        if (manaToRestore > 0)
        {
            playerController.RestoreMana(manaToRestore);
            Debug.Log($"Used {itemName} to restore {manaToRestore} Mana.");
            wasUsed = true; // Giả sử mana luôn có thể hồi
        }

        // Chỉ xóa vật phẩm nếu nó thực sự đã được sử dụng (máu hoặc mana được hồi)
        if (wasUsed)
        {
            InventoryManager.instance.Remove(this);
        }
    }
}