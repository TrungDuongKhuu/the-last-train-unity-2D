using UnityEngine;

public enum StatType { WalkSpeed, RunSpeed, DamageMultiplier }

[CreateAssetMenu(fileName = "New Buff Effect", menuName = "Inventory/Buff Effect")]
public class BuffEffect : ScriptableObject
{
    [Header("Buff Information")]
    public string effectName;
    public float duration; // Thời gian hiệu lực (giây). Đặt là 0 nếu vĩnh viễn.

    [Header("Stat Changes")]
    public StatType statToModify;
    public float value; // Giá trị cộng thêm
    public bool isMultiplier; // Đánh dấu nếu giá trị này là số nhân (ví dụ: tăng 50% tốc độ)
}
