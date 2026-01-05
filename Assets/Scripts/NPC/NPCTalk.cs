using UnityEngine;

public class NPCTalk : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;

    [Header("Animator UI tương tác (E để nói chuyện)")]
    public Animator interactAnim;

    // Trạng thái hiện tại của icon
    private bool interactVisible = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;

        }

        if (anim != null)
            anim.Play("Idle");
        // KHÔNG đụng gì đến interactAnim ở đây
    }

    private void OnDisable()
    {
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        // KHÔNG gọi Close ở đây nữa
    }

    // Gọi khi Player vào vùng nói chuyện
    public void ShowInteract()
    {
        if (interactAnim != null && !interactVisible)
        {
            interactAnim.Play("Open");
            interactVisible = true;
        }
    }

    // Gọi khi Player rời vùng nói chuyện
    public void HideInteract()
    {
        if (interactAnim != null && interactVisible)
        {
            interactAnim.Play("Close");
            interactVisible = false;
        }
    }
}
