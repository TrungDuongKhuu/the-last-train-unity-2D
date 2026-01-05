using UnityEngine;

public class CourtEndTrigger : MonoBehaviour
{
    [Header("Tên tag của Player")]
    public string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("[CourtEndTrigger] Player entered → Checking ending...");
            GameManager.Instance.LoadEnding();
        }
    }
}
