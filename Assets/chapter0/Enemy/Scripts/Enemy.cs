using UnityEngine;

public class Enemy : MonoBehaviour
{
    public StateMachine StateMachine { get; private set; }
    public PatrolState PatrolState { get; private set; }
    public ChaseState ChaseState { get; private set; }
    public AttackState AttackState { get; private set; }
    public StunState StunState { get; private set; }

    public Animator Animator { get; private set; }

    [HideInInspector] public bool isStunned = false;

    public Transform player;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public Transform[] patrolPoints;
    public float attackCooldown = 2f;
    public int damage = 10;

    [Header("Audio")]
    public AudioClip idleSound; // Tiếng kêu khi tuần tra/nhàn rỗi
    public AudioClip chaseSound; // Tiếng kêu khi đuổi theo
    public AudioClip attackSound; // Tiếng tấn công
    [Range(0f, 1f)] public float enemySoundVolume = 0.6f;
    public float idleSoundInterval = 5f; // Khoảng thời gian giữa các tiếng kêu khi tuần tra

    [Header("Auto Patrol Settings (if no patrol points)")]
    public bool useAutoPatrol = true;
    public bool patrolOnXAxis = true; // true = di chuyển theo trục X, false = theo trục Y
    public float patrolDistance = 5f; // Khoảng cách giữa 2 điểm tuần tra

    [Header("Leash Settings")]
    public bool useLeash = true; // Giới hạn khoảng cách enemy có thể đi xa
    public float leashDistance = 15f; // Khoảng cách tối đa từ vị trí ban đầu
    public float resetSpeed = 8f; // Tốc độ khi reset về vị trí ban đầu

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Vector3 startingPosition;
    [HideInInspector] public float lastAttackTime;
    [HideInInspector] public AudioSource audioSource;
    [HideInInspector] public float lastIdleSoundTime;

    private void Awake()
    {
        StateMachine = new StateMachine();
        rb = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
        startingPosition = transform.position;
        
        // Tạo AudioSource nếu chưa có
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.minDistance = 5f;
            audioSource.maxDistance = 20f;
        }

        // Khóa rotation để enemy không bị xoay vòng
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        PatrolState = new PatrolState(this, StateMachine);
        ChaseState = new ChaseState(this, StateMachine);
        AttackState = new AttackState(this, StateMachine);
        StunState = new StunState(this, StateMachine);
    }

    private void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        // Kiểm tra và tạo patrol points nếu cần
        bool needsPatrolPoints = patrolPoints == null || patrolPoints.Length == 0 || patrolPoints[0] == null;
        
        if (useAutoPatrol && needsPatrolPoints)
        {
            Debug.Log($"[{gameObject.name}] Generating auto patrol points...");
            GenerateRandomPatrolPoints();
        }
        else if (!needsPatrolPoints)
        {
            Debug.Log($"[{gameObject.name}] Using {patrolPoints.Length} manual patrol points");
        }

        // Khởi tạo StateMachine SAU khi đã có patrol points
        StateMachine.Initialize(PatrolState);
    }

    private void GenerateRandomPatrolPoints()
    {
        // Tạo 2 điểm tuần tra tự động
        GameObject patrolParent = new GameObject($"{gameObject.name}_PatrolPoints");
        patrolParent.transform.position = startingPosition;

        Transform point1 = new GameObject("PatrolPoint1").transform;
        Transform point2 = new GameObject("PatrolPoint2").transform;

        point1.parent = patrolParent.transform;
        point2.parent = patrolParent.transform;

        if (patrolOnXAxis)
        {
            // Di chuyển theo trục X (trái phải)
            point1.position = startingPosition + new Vector3(-patrolDistance / 2f, 0, 0);
            point2.position = startingPosition + new Vector3(patrolDistance / 2f, 0, 0);
        }
        else
        {
            // Di chuyển theo trục Y (lên xuống)
            point1.position = startingPosition + new Vector3(0, -patrolDistance / 2f, 0);
            point2.position = startingPosition + new Vector3(0, patrolDistance / 2f, 0);
        }

        patrolPoints = new Transform[] { point1, point2 };

        Debug.Log($"Auto-generated patrol points for {gameObject.name} on {(patrolOnXAxis ? "X" : "Y")} axis with distance {patrolDistance}");
    }

    private void Update()
    {
        StateMachine.CurrentState.HandleUpdate();

        // Kiểm tra khoảng cách với điểm ban đầu
        if (useLeash)
        {
            CheckLeashDistance();
        }
    }

    private void FixedUpdate()
    {
        StateMachine.CurrentState.HandleFixedUpdate();
    }

    public void ApplyStun(float duration)
    {
        if (!isStunned)
        {
            StunState.SetStun(duration, StateMachine.CurrentState);
            StateMachine.ChangeState(StunState);
        }
    }

    private void CheckLeashDistance()
    {
        float distanceFromStart = Vector3.Distance(transform.position, startingPosition);
        
        if (distanceFromStart > leashDistance)
        {
            // Đi quá xa, reset về vị trí ban đầu
            Debug.Log($"[{gameObject.name}] Too far from home! Resetting position...");
            ResetToStartPosition();
        }
    }

    public void ResetToStartPosition()
    {
        // Dừng tất cả chuyển động
        rb.linearVelocity = Vector2.zero;
        
        // Teleport về vị trí ban đầu
        transform.position = startingPosition;
        
        // Reset về trạng thái Patrol
        StateMachine.ChangeState(PatrolState);
        
        Debug.Log($"[{gameObject.name}] Reset to starting position");
    }

    // ------------------------
    // Audio Methods
    // ------------------------
    public void PlayIdleSound()
    {
        if (idleSound != null && audioSource != null && Time.time >= lastIdleSoundTime + idleSoundInterval)
        {
            audioSource.PlayOneShot(idleSound, enemySoundVolume);
            lastIdleSoundTime = Time.time;
        }
    }

    public void PlayChaseSound()
    {
        if (chaseSound != null && audioSource != null && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(chaseSound, enemySoundVolume);
        }
    }

    public void PlayAttackSound()
    {
        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound, enemySoundVolume);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
