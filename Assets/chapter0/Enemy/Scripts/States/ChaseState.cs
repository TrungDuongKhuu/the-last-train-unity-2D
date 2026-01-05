using UnityEngine;

public class ChaseState : EnemyState
{
    public ChaseState(Enemy enemy, StateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void Enter()
    {
        // Phát tiếng kêu khi bắt đầu đuổi theo
        enemy.PlayChaseSound();
    }

    public override void HandleUpdate()
    {
        if (enemy.isStunned) return;

        float distanceToPlayer = Vector2.Distance(enemy.transform.position, enemy.player.position);

        if (distanceToPlayer <= enemy.attackRange)
        {
            stateMachine.ChangeState(enemy.AttackState);
        }
        else if (distanceToPlayer > enemy.detectionRange)
        {
            stateMachine.ChangeState(enemy.PatrolState);
        }
    }

    public override void HandleFixedUpdate()
    {
        Vector2 direction = (enemy.player.position - enemy.transform.position).normalized;
        enemy.rb.linearVelocity = direction * enemy.chaseSpeed;
        enemy.Animator.SetFloat("moveX", direction.x);
        enemy.Animator.SetFloat("moveY", direction.y);
    }

    public override void Exit()
    {
        enemy.rb.linearVelocity = Vector2.zero;
        enemy.Animator.SetFloat("moveX", 0);
        enemy.Animator.SetFloat("moveY", 0);
    }
}
