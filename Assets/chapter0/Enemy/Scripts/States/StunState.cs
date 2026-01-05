using UnityEngine;

public class StunState : EnemyState
{
    private float stunDuration;
    private float stunTimer;
    private EnemyState stateToReturnTo;

    public StunState(Enemy enemy, StateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        enemy.isStunned = true;
        stunTimer = 0f;
        enemy.rb.linearVelocity = Vector2.zero; // Dừng vận tốc vật lý

        // QUAN TRỌNG: Tắt Animator để nó không ghi đè di chuyển
        if (enemy.Animator != null)
        {
            enemy.Animator.enabled = false;
        }

        Debug.Log($"{enemy.gameObject.name} bị choáng trong {stunDuration} giây!");
    }

    public override void HandleUpdate()
    {
        base.HandleUpdate();
        stunTimer += Time.deltaTime;

        if (stunTimer >= stunDuration)
        {
            stateMachine.ChangeState(stateToReturnTo);
        }
    }

    public override void HandleFixedUpdate()
    {
        // Luôn giữ vận tốc bằng 0 để đảm bảo đứng yên tuyệt đối
        enemy.rb.linearVelocity = Vector2.zero;
    }

    public override void Exit()
    {
        base.Exit();
        enemy.isStunned = false;

        // QUAN TRỌNG: Bật lại Animator khi hết choáng
        if (enemy.Animator != null)
        {
            enemy.Animator.enabled = true;
        }

        Debug.Log($"{enemy.gameObject.name} đã hết bị choáng.");
    }

    public void SetStun(float duration, EnemyState returnState)
    {
        this.stunDuration = duration;
        if (returnState == null || returnState == this)
        {
            this.stateToReturnTo = enemy.PatrolState;
        }
        else
        {
            this.stateToReturnTo = returnState;
        }
    }
}