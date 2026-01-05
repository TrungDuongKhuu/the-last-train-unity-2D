using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Weapon Item")]
public class WeaponItemData : ItemData
{
    [Header("Weapon Properties")]
    public int damage = 10;
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    public bool canStunEnemies = false;
    
    [Header("Stun Effect (if applicable)")]
    public float stunDuration = 3f;
    public float stunRadius = 5f; // Phạm vi ảnh hưởng của hiệu ứng stun
    
    [Header("Damage Settings")]
    public bool isAreaDamage = false; // Gây sát thương theo vùng hay đơn mục tiêu
    public float damageRadius = 3f; // Bán kính gây sát thương nếu isAreaDamage = true
    
    [Header("Visual & Audio")]
    public GameObject attackEffectPrefab; // Hiệu ứng khi tấn công
    public AudioClip attackSound;

    [Header("Durability")]
    public int maxUses = 10; // Số lần sử dụng tối đa
    public bool hasDurability = true; // Có hạn sử dụng hay không

    [HideInInspector] public int remainingUses; // Số lần sử dụng còn lại
    private float lastAttackTime = 0f;

    public override void Use(Controller playerController)
    {
        // Khởi tạo số lần sử dụng lần đầu tiên
        if (remainingUses == 0 && hasDurability)
        {
            remainingUses = maxUses;
        }

        // Kiểm tra cooldown
        if (Time.time < lastAttackTime + attackCooldown)
        {
            Debug.Log($"{itemName} is on cooldown!");
            return;
        }

        // Kiểm tra số lần sử dụng còn lại
        if (hasDurability && remainingUses <= 0)
        {
            Debug.Log($"{itemName} has no uses left!");
            InventoryManager.instance.Remove(this);
            return;
        }

        lastAttackTime = Time.time;

        // Lấy vị trí người chơi
        Vector3 playerPosition = playerController.transform.position;

        if (isAreaDamage)
        {
            // Gây sát thương theo vùng
            DealAreaDamage(playerPosition);
        }
        else
        {
            // Tấn công mục tiêu gần nhất
            DealSingleTargetDamage(playerPosition);
        }

        // Áp dụng hiệu ứng stun nếu vũ khí có khả năng này
        if (canStunEnemies)
        {
            ApplyStunEffect(playerPosition);
        }

        // Phát hiệu ứng và âm thanh
        PlayAttackEffects(playerPosition);

        // Giảm số lần sử dụng
        if (hasDurability)
        {
            remainingUses--;
            Debug.Log($"Used weapon: {itemName}. Remaining uses: {remainingUses}/{maxUses}");

            // Xóa vũ khí nếu hết số lần sử dụng
            if (remainingUses <= 0)
            {
                Debug.Log($"{itemName} has been used up and removed from inventory.");
                InventoryManager.instance.Remove(this);
            }
        }
        else
        {
            Debug.Log($"Used weapon: {itemName} (unlimited uses)");
        }
    }

    private void DealSingleTargetDamage(Vector3 position)
    {
        // Tìm enemy gần nhất trong phạm vi tấn công
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, attackRange);
        Enemy closestEnemy = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                float distance = Vector2.Distance(position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
        }

        // Gây sát thương cho enemy gần nhất
        if (closestEnemy != null)
        {
            DealDamageToEnemy(closestEnemy);
            Debug.Log($"Dealt {damage} damage to closest enemy.");
        }
        else
        {
            Debug.Log("No enemy in range to attack.");
        }
    }

    private void DealAreaDamage(Vector3 position)
    {
        // Tìm tất cả enemy trong bán kính gây sát thương
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, damageRadius);
        int enemiesHit = 0;

        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                DealDamageToEnemy(enemy);
                enemiesHit++;
            }
        }

        Debug.Log($"Area attack hit {enemiesHit} enemies for {damage} damage each.");
    }

    private void DealDamageToEnemy(Enemy enemy)
    {
        // Bạn cần thêm phương thức TakeDamage vào class Enemy
        // enemy.TakeDamage(damage);
        
        // Tạm thời chỉ log, bạn sẽ cần implement TakeDamage trong Enemy.cs
        Debug.Log($"Enemy at {enemy.transform.position} takes {damage} damage!");
    }

    private void ApplyStunEffect(Vector3 position)
    {
        // Tìm tất cả enemy trong bán kính stun
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, stunRadius);
        int enemiesStunned = 0;

        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.ApplyStun(stunDuration);
                enemiesStunned++;
            }
        }

        Debug.Log($"Stunned {enemiesStunned} enemies for {stunDuration} seconds.");
    }

    private void PlayAttackEffects(Vector3 position)
    {
        // Tạo hiệu ứng tấn công
        if (attackEffectPrefab != null)
        {
            GameObject effect = Instantiate(attackEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 2f); // Xóa hiệu ứng sau 2 giây
        }

        // Phát âm thanh
        if (attackSound != null)
        {
            AudioSource.PlayClipAtPoint(attackSound, position);
        }
    }

    // Phương thức để lấy số lần sử dụng còn lại
    public int GetRemainingUses()
    {
        return hasDurability ? remainingUses : -1; // -1 nghĩa là không giới hạn
    }

    // Phương thức để đặt lại số lần sử dụng (dùng khi nhặt vũ khí mới)
    public void ResetUses()
    {
        remainingUses = maxUses;
    }

    // Phương thức helper để vẽ gizmos trong Scene view
    public void DrawGizmos(Vector3 position)
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(position, attackRange);
        
        if (isAreaDamage)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(position, damageRadius);
        }
        
        if (canStunEnemies)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(position, stunRadius);
        }
    }
}
