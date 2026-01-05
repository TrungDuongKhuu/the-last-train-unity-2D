// File: Assets/Interaction/Items/ItemScripts/StunWeaponData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New Stun Weapon", menuName = "Inventory/Stun Weapon")]
public class StunWeaponData : PermanentItemData
{
    [Header("Stun Weapon Stats")]
    [Tooltip("Thời gian làm choáng kẻ địch (tính bằng giây).")]
    public float stunDuration = 3f;

    [Tooltip("Phạm vi tác dụng của vũ khí.")]
    public float effectRange = 2f;

    [Tooltip("Thời gian chờ giữa các lần sử dụng.")]
    public float cooldown = 1f;

    private float lastUseTimestamp = -999f;
    private bool wasOnCooldown = false; // Biến mới để theo dõi

    public override void Use(Controller playerController)
    {
        // Kiểm tra thời gian hồi chiêu
        if (Time.time < lastUseTimestamp + cooldown)
        {
            if (!wasOnCooldown)
            {
                Debug.Log("Vũ khí đang trong thời gian hồi...");
                wasOnCooldown = true;
            }
            return;
        }

        // Nếu code chạy đến đây, nghĩa là đã hết thời gian hồi
        if (wasOnCooldown)
        {
            Debug.Log("Vũ khí đã sẵn sàng!");
            wasOnCooldown = false;
        }

        lastUseTimestamp = Time.time;

        // ... (phần code còn lại của hàm Use giữ nguyên) ...
        Vector2 attackPoint = (Vector2)playerController.transform.position + new Vector2(playerController.transform.localScale.x * (effectRange / 2), 0.5f);
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint, effectRange / 2, LayerMask.GetMask("Enemy"));
        Debug.Log($"Sử dụng vũ khí choáng. Tìm thấy {hitEnemies.Length} kẻ địch.");
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            if (enemyCollider.TryGetComponent<Enemy>(out Enemy enemy))
            {
                enemy.ApplyStun(stunDuration);
            }
        }
    }
}