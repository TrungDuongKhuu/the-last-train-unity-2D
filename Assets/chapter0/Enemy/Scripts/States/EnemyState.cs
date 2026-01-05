using UnityEngine;

public abstract class EnemyState
{
    protected Enemy enemy;
    protected StateMachine stateMachine;

    public EnemyState(Enemy enemy, StateMachine stateMachine)
    {
        this.enemy = enemy;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter() { }
    public virtual void HandleUpdate() { }
    public virtual void HandleFixedUpdate() { }
    public virtual void Exit() { }
}
