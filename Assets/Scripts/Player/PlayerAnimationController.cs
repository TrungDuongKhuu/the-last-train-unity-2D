using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;
    private Controller controller;

    private enum PlayerState
    {
        Idle,
        Walking,
        Running
    }

    private PlayerState currentState = PlayerState.Idle;

    void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<Controller>();

        if (controller == null)
        {
            Debug.LogError("[PlayerAnimationController] Không tìm thấy Controller.cs trên Player!");
        }
    }

    void Update()
    {
        if (controller == null) return;

        HandleState();
        ApplyAnimation();
    }

    // ---------------------------------------------------------
    // XỬ LÝ TRẠNG THÁI DỰA TRÊN Controller.cs
    // ---------------------------------------------------------
    void HandleState()
    {
        // Nếu đang di chuyển
        if (controller.moveInput.sqrMagnitude > 0.01f)
        {
            currentState = controller.isRun ? PlayerState.Running : PlayerState.Walking;
        }
        else
        {
            currentState = PlayerState.Idle;
        }
    }

    // ---------------------------------------------------------
    // ÁP DỤNG ANIMATION
    // ---------------------------------------------------------
    void ApplyAnimation()
    {
        Vector2 move = controller.moveInput;

        if (currentState == PlayerState.Idle)
        {
            // Khi đứng yên, dùng hướng cuối
            animator.SetFloat("lastX", controller.lastX);
            animator.SetFloat("lastY", controller.lastY);
        }
        else
        {
            // Khi đang di chuyển, cập nhật hướng và lưu hướng cuối
            animator.SetFloat("moveX", move.x);
            animator.SetFloat("moveY", move.y);

            // Lưu hướng cuối
            controller.lastX = move.x;
            controller.lastY = move.y;
        }

        // Speed là dùng để blend tree (0 = idle, >0 = moving)
        animator.SetFloat("Speed", move.sqrMagnitude);

        animator.SetBool("isWalking", currentState == PlayerState.Walking);
        animator.SetBool("isRunning", currentState == PlayerState.Running);
    }
}
