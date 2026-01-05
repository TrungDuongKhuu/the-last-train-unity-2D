using UnityEngine;

// Lớp cơ sở cho tất cả vật phẩm, giờ sẽ có hành vi
public abstract class ItemData : ScriptableObject
{
    [Header("Item Information")]
    public string itemName;
    public Sprite icon;
    [TextArea(3, 5)]
    public string description;

    // Phương thức trừu tượng, buộc các vật phẩm con phải định nghĩa cách sử dụng
    public abstract void Use(Controller playerController);
}