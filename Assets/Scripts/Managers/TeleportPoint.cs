using UnityEngine;

public class TeleportPoint : MonoBehaviour
{
    public Transform target;
    public GameObject player;

    public void Teleport()
    {
        var rb = player.GetComponent<Rigidbody2D>();
        rb.position = target.position; // dùng physics teleport
        rb.linearVelocity = Vector2.zero;

    }
}
