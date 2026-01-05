using UnityEngine;

public class AttackState : EnemyState
{
    public AttackState(Enemy enemy, StateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void Enter()
    {
        enemy.rb.linearVelocity = Vector2.zero;

        // Make the enemy face the player
        Vector2 directionToPlayer = (enemy.player.position - enemy.transform.position).normalized;
        enemy.Animator.SetFloat("moveX", directionToPlayer.x);
        enemy.Animator.SetFloat("moveY", directionToPlayer.y);
    }

    public override void HandleUpdate()
    {
        if (enemy.isStunned) return;

        if (Vector2.Distance(enemy.transform.position, enemy.player.position) > enemy.attackRange)
        {
            stateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        if (Time.time >= enemy.lastAttackTime + enemy.attackCooldown)
        {
            Attack();
        }
    }

    private void Attack()
    {
        enemy.lastAttackTime = Time.time;
        
        // Cập nhật hướng ngay trước khi trigger attack để đảm bảo Blend Tree chọn đúng animation
        Vector2 directionToPlayer = (enemy.player.position - enemy.transform.position).normalized;
        enemy.Animator.SetFloat("moveX", directionToPlayer.x);
        enemy.Animator.SetFloat("moveY", directionToPlayer.y);
        
        // Trigger attack NGAY SAU khi set direction
        enemy.Animator.SetTrigger("Attack");
        
        // Phát âm thanh tấn công
        enemy.PlayAttackSound();

        Debug.Log($"Enemy attacks! Direction: X={directionToPlayer.x:F2}, Y={directionToPlayer.y:F2}");
        
        // The rest of your attack logic
        Controller playerController = enemy.player.GetComponent<Controller>();
        if (playerController != null)
        {
            playerController.TakeDamage(enemy.damage);
        }
    }
}
