using System.Collections;
using UnityEngine;

public class NPCPatrol : MonoBehaviour
{
    [Header("Patrol Points (nhập vị trí X,Y tùy ý)")]
    public Vector2[] patrolPoints;

    [Header("Settings")]
    public float speed = 2f;
    public float pauseDuration = 1f;

    private int currentIndex = 0;
    private Vector2 target;
    private bool isPaused = false;

    private Rigidbody2D rb;
    private Animator anim;

    private enum NPCState { Idle, Walking }
    private NPCState currentState = NPCState.Idle;

    // Hướng cuối cùng (giống Player)
    private float lastX = 0;
    private float lastY = -1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();

        if (patrolPoints.Length > 0)
            target = patrolPoints[0];
    }

    void Update()
    {
        // ======================
        // 🔥 IDLE
        // ======================
        if (isPaused)
        {
            rb.linearVelocity = Vector2.zero;
            currentState = NPCState.Idle;

            anim.SetBool("Walking", false);
            anim.SetFloat("MoveX", 0);
            anim.SetFloat("MoveY", 0);
            anim.SetFloat("LastX", lastX);
            anim.SetFloat("LastY", lastY);

            return;
        }

        // ======================
        // 🔥 WALK
        // ======================
        Vector2 direction = ((Vector3)target - transform.position).normalized;
        rb.linearVelocity = direction * speed;
        currentState = NPCState.Walking;

        anim.SetBool("Walking", true);
        anim.SetFloat("MoveX", direction.x);
        anim.SetFloat("MoveY", direction.y);

        // Lưu hướng cuối
        if (direction.sqrMagnitude > 0.01f)
        {
            lastX = direction.x;
            lastY = direction.y;
            anim.SetFloat("LastX", lastX);
            anim.SetFloat("LastY", lastY);
        }

        // Đến điểm tuần tra
        if (Vector2.Distance(transform.position, target) < 0.1f)
            StartCoroutine(NextPoint());
    }

    IEnumerator NextPoint()
    {
        isPaused = true;

        anim.SetBool("Walking", false);
        anim.SetFloat("MoveX", 0);
        anim.SetFloat("MoveY", 0);
        anim.SetFloat("LastX", lastX);
        anim.SetFloat("LastY", lastY);

        yield return new WaitForSeconds(pauseDuration);

        currentIndex = (currentIndex + 1) % patrolPoints.Length;
        target = patrolPoints[currentIndex];

        isPaused = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        if (patrolPoints == null || patrolPoints.Length < 2)
            return;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            Gizmos.DrawSphere(patrolPoints[i], 0.1f);

            if (i < patrolPoints.Length - 1)
                Gizmos.DrawLine(patrolPoints[i], patrolPoints[i + 1]);
        }

        Gizmos.DrawLine(patrolPoints[patrolPoints.Length - 1], patrolPoints[0]);
    }
}
