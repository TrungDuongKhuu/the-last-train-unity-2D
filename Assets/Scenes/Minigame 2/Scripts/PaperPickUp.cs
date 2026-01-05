using UnityEngine;

public class PaperPiecePickup : MonoBehaviour
{
    [Header("ID của mảnh giấy (duy nhất trong scene)")]
    public string paperID = "Paper_01";

    [Header("UI gợi ý nhấn E")]
    [SerializeField] private GameObject interactUI;

    private bool playerInRange = false;
    private bool collected = false;

    private string prefsKey => $"PaperPiece_{paperID}_Collected";

    private void Start()
    {
        // Kiểm tra xem mảnh này đã được nhặt từ trước chưa
        if (PlayerPrefs.GetInt(prefsKey, 0) == 1)
        {
            collected = true;
            gameObject.SetActive(false);
            return;
        }

        if (interactUI != null)
            interactUI.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || collected) return;

        playerInRange = true;

        if (interactUI != null)
            interactUI.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        if (interactUI != null)
            interactUI.SetActive(false);
    }

    private void Update()
    {
        if (!playerInRange || collected) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Collect();
        }
    }

    private void Collect()
    {
        // Lưu tiến độ cho mảnh giấy
        PlayerPrefs.SetInt(prefsKey, 1);
        PlayerPrefs.Save();

        // Báo cho PaperManager
        if (PaperManager.Instance != null)
        {
            PaperManager.Instance.CollectPiece();
        }

        collected = true;

        if (interactUI != null)
            interactUI.SetActive(false);

        gameObject.SetActive(false);
    }
}
