using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon; // Kéo ItemIcon vào đây
    private ItemData item;
    private Image slotBackground; // Background/border của chính slot này

    private void Awake()
    {
        // Lấy Image component của chính object này (là background/border)
        slotBackground = GetComponent<Image>();
        
        // Đặt màu mặc định cho slot trống
        if (slotBackground != null)
        {
            slotBackground.color = new Color32(128, 128, 128, 255); // Màu xám mặc định cho slot trống
        }
    }

    public void AddItem(ItemData newItem)
    {
        item = newItem;
        icon.sprite = item.icon;
        icon.enabled = true;
    }

    public void ClearSlot()
    {
        item = null;
        icon.sprite = null;
        icon.enabled = false;
        
        // Đặt lại màu mặc định khi slot trống
        if (slotBackground != null)
        {
            slotBackground.color = new Color32(128, 128, 128, 255); // Màu xám mặc định
        }
    }

    // Hàm mới để đổi màu border
    public void SetBorderColor(bool isSelected)
    {
        if (slotBackground == null) return;

        if (item != null) // Chỉ đổi màu khi có item
        {
            if (isSelected)
            {
                // Màu được chọn: màu xanh lá tươi tương phản
                slotBackground.color = new Color32(0, 255, 0, 255); // rgba(0,255,0,255) - xanh lá
            }
            else
            {
                // Màu không được chọn: màu đỏ đậm
                slotBackground.color = new Color32(107, 8, 8, 255); // rgba(107,8,8,255) - đỏ đậm
            }
        }
        else // Nếu slot trống, luôn là màu xám
        {
            slotBackground.color = new Color32(128, 128, 128, 255); // Màu xám mặc định
        }
    }
}