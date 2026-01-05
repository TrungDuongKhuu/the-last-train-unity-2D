using UnityEngine;

public class PatrolState : EnemyState
{
    private int currentPatrolPointIndex;

    public PatrolState(Enemy enemy, StateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void Enter()
    {
        currentPatrolPointIndex = 0;
    }

    public override void HandleUpdate()
    {
        if (enemy.isStunned) return;

        if (Vector2.Distance(enemy.transform.position, enemy.player.position) < enemy.detectionRange)
        {
            stateMachine.ChangeState(enemy.ChaseState);
        }
    }

    public override void HandleFixedUpdate()
    {
        if (enemy.patrolPoints == null || enemy.patrolPoints.Length == 0)
        {
            enemy.rb.linearVelocity = Vector2.zero;
            enemy.Animator.SetFloat("moveX", 0);
            enemy.Animator.SetFloat("moveY", 0);
            enemy.PlayIdleSound(); // Phát tiếng kêu nhàn rỗi
            return;
        }

        Transform targetPoint = enemy.patrolPoints[currentPatrolPointIndex];
        Vector2 direction = (targetPoint.position - enemy.transform.position).normalized;
        enemy.rb.linearVelocity = direction * enemy.patrolSpeed;

        enemy.Animator.SetFloat("moveX", direction.x);
        enemy.Animator.SetFloat("moveY", direction.y);

        if (Vector2.Distance(enemy.transform.position, targetPoint.position) < 0.5f)
        {
            currentPatrolPointIndex = (currentPatrolPointIndex + 1) % enemy.patrolPoints.Length;
        }
        
        // Phát tiếng kêu theo khoảng thời gian
        enemy.PlayIdleSound();
    }

    public override void Exit()
    {
        enemy.Animator.SetFloat("moveX", 0);
        enemy.Animator.SetFloat("moveY", 0);
    }
}
