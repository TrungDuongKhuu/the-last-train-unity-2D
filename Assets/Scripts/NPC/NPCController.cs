using UnityEngine;

public class NPCController : MonoBehaviour
{
    public enum NPCState { Patrol, Wander, Talk }
    public NPCState currentState = NPCState.Patrol;
    private NPCState defaultState;

    public NPCPatrol patrol;
    public NPCWander wander;
    public NPCTalk talk;  // script pop up icon

    void Start()
    {
        defaultState = currentState;
        SwitchState(currentState);
    }

    public void SwitchState(NPCState newState)
    {
        currentState = newState;

        // Bật/tắt logic di chuyển
        if (patrol != null) patrol.enabled = (newState == NPCState.Patrol);
        if (wander != null) wander.enabled = (newState == NPCState.Wander);

        // Bật / tắt NPCTalk theo state
        if (talk != null)
            talk.enabled = (newState == NPCState.Talk);

        if (newState == NPCState.Talk)
        {
            talk.ShowInteract();
            StopMovement();
        }
        else
        {
            talk.HideInteract();
        }
    }


    private void StopMovement()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Animator anim = GetComponentInChildren<Animator>();

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (anim != null)
        {
            anim.SetBool("Walking", false);
            anim.SetFloat("MoveX", 0);
            anim.SetFloat("MoveY", 0);
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
            SwitchState(NPCState.Talk);
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
            SwitchState(defaultState);
    }
}
