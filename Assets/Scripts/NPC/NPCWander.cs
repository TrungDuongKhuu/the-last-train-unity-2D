using System.Collections;
using UnityEngine;

public class NPCWander : MonoBehaviour
{
    [Header("Wander Settings")]
    public float wanderWidth = 5f;
    public float wanderHeight = 5f;
    public Vector2 startingPosition;

    [Header("Movement Settings")]
    public float pauseDuration = 1f;
    public float speed = 2f;
    public Vector2 target;

    private Rigidbody2D rb;
    private Animator anim;
    private bool isPaused;

    public LayerMask obstacleLayer;

    // Hướng cuối để Idle đúng hướng
    private float lastX = 0;
    private float lastY = -1;

    private BoxCollider2D box;   // để lấy kích thước collider

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        box = GetComponent<BoxCollider2D>();
    }

    private void OnEnable()
    {
        StartCoroutine(PauseAndPickNewDestination());
    }

    private void Update()
    {
        if (!enabled)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("Walking", false);
            return;
        }

        if (isPaused)
        {
            rb.linearVelocity = Vector2.zero;

            if (anim != null)
            {
                anim.SetBool("Walking", false);
                anim.SetFloat("MoveX", 0);
                anim.SetFloat("MoveY", 0);
                anim.SetFloat("LastX", lastX);
                anim.SetFloat("LastY", lastY);
            }

            return;
        }

        // Đến target → nghỉ → chọn target mới
        if (Vector2.Distance(transform.position, target) < 0.15f)
        {
            StartCoroutine(PauseAndPickNewDestination());
            return;
        }

        Move();
    }

    // =========================================
    //          HỆ THỐNG 4-SIDE RAYCAST
    // =========================================
    private bool CheckBlocked(Vector2 direction)
    {
        float dist = 0.6f;

        // ================================
        //  GÓT CHÂN (anchor chính)
        // ================================
        float footY = -box.size.y * 0.5f + 0.02f;    // thêm 0.02f để không lọt collider
        Vector2 foot = (Vector2)transform.position + new Vector2(0, footY);
        float bottomfootY = transform.position.y + box.offset.y - (box.size.y * 0.5f);
        Vector2 bottomfoot = new Vector2(transform.position.x, bottomfootY);

        // ================================
        // 4 RAY, TẤT CẢ TỪ GÓT CHÂN
        // ================================

        // Ray dưới chân
        Vector2 bottom = foot;

        // Ray trên chân (nhích lên chút xíu nhưng vẫn ở foot-level)
        Vector2 top = bottomfoot + new Vector2(0, 0.2f);

        // Ray trái & phải từ đúng mép gót chân
        Vector2 left = foot + new Vector2(-box.size.x * 0.5f, 0);
        Vector2 right = foot + new Vector2(box.size.x * 0.5f, 0);

        RaycastHit2D hitBottom = Physics2D.Raycast(bottom, direction, dist, obstacleLayer);
        RaycastHit2D hitTop = Physics2D.Raycast(top, direction, dist, obstacleLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(left, direction, dist, obstacleLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(right, direction, dist, obstacleLayer);

        // Debug rays
        Debug.DrawRay(bottom, direction * dist, Color.red);     // dưới chân
        Debug.DrawRay(top, direction * dist, Color.yellow);      // trên chân (ankle)
        Debug.DrawRay(left, direction * dist, Color.green);      // trái
        Debug.DrawRay(right, direction * dist, Color.blue);      // phải

        return hitBottom || hitTop || hitLeft || hitRight;
    }



    // =========================================
    //                 MOVE
    // =========================================
    private void Move()
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;

        // Check block với 4 raycast
        bool blocked = CheckBlocked(direction);

        if (blocked)
        {
            rb.linearVelocity = Vector2.zero;

            if (anim != null)
            {
                anim.SetBool("Walking", false);
                anim.SetFloat("MoveX", 0);
                anim.SetFloat("MoveY", 0);
                anim.SetFloat("LastX", lastX);
                anim.SetFloat("LastY", lastY);
            }

            StartCoroutine(PauseAndPickNewDestination());
            return;
        }

        // Di chuyển
        rb.linearVelocity = direction * speed;

        // Animation
        if (anim != null)
        {
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
        }
    }

    // =========================================
    //      Khi đụng NPC hoặc vật thể → đổi hướng
    // =========================================
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("NPC") ||
            ((1 << col.collider.gameObject.layer) & obstacleLayer) != 0)
        {
            // NGĂN BỊ ĐẨY
            rb.linearVelocity = Vector2.zero;
            rb.linearVelocity = Vector2.zero;       // <- thêm dòng này

            StartCoroutine(PauseAndPickNewDestination());
        }
    }


    IEnumerator PauseAndPickNewDestination()
    {
        isPaused = true;

        if (anim != null)
        {
            anim.SetBool("Walking", false);
            anim.SetFloat("MoveX", 0);
            anim.SetFloat("MoveY", 0);
            anim.SetFloat("LastX", lastX);
            anim.SetFloat("LastY", lastY);
        }

        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(pauseDuration);

        target = GetRandomTarget();
        isPaused = false;
    }

    private Vector2 GetRandomTarget()
    {
        float halfWidth = wanderWidth / 2f;
        float halfHeight = wanderHeight / 2f;

        int edge = Random.Range(0, 4);

        return edge switch
        {
            0 => new Vector2(startingPosition.x - halfWidth, Random.Range(startingPosition.y - halfHeight, startingPosition.y + halfHeight)),
            1 => new Vector2(startingPosition.x + halfWidth, Random.Range(startingPosition.y - halfHeight, startingPosition.y + halfHeight)),
            2 => new Vector2(Random.Range(startingPosition.x - halfWidth, startingPosition.x + halfWidth), startingPosition.y - halfHeight),
            3 => new Vector2(Random.Range(startingPosition.x - halfWidth, startingPosition.x + halfWidth), startingPosition.y + halfHeight),
            _ => startingPosition
        };
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(startingPosition, new Vector3(wanderWidth, wanderHeight, 0));
    }
}
