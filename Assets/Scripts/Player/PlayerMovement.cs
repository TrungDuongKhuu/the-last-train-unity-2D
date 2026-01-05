using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 5f;

    private Rigidbody2D rb;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LastMoveDir { get; private set; }
    public bool IsRunning { get; private set; }

    // 👇 thêm 2 biến này
    public float LastX { get; private set; }
    public float LastY { get; private set; }

    // 🔽 thêm biến điều khiển input
    private bool inputEnabled = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 🔽 Nếu bị tắt input (do cutscene) → không đọc phím
        if (!inputEnabled)
        {
            MoveInput = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        MoveInput = new Vector2(moveX, moveY);

        if (MoveInput.sqrMagnitude > 1f)
            MoveInput.Normalize();

        IsRunning = Input.GetKey(KeyCode.LeftShift);

        if (MoveInput.sqrMagnitude > 0.01f)
        {
            LastMoveDir = MoveInput.normalized;
            LastX = LastMoveDir.x;
            LastY = LastMoveDir.y;
        }
    }

    void FixedUpdate()
    {
        float speed = IsRunning ? runSpeed : walkSpeed;
        rb.linearVelocity = MoveInput * speed;
    }

    // 🔽 Thêm hàm này để CutsceneController có thể bật/tắt điều khiển
    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;

        // Nếu tắt input thì cũng dừng player ngay
        if (!enabled)
        {
            MoveInput = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void DisableControl()
    {
        inputEnabled = false;
        MoveInput = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }
    public void EnableControl()
    {
        inputEnabled = true;
    }
}
