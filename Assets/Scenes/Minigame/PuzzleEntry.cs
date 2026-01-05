using UnityEngine;

public class PuzzleEntry : MonoBehaviour
{
    [Header("Puzzle Scene Settings")]
    [SerializeField] private string puzzleSceneName;           // Tên scene puzzle sẽ load
    [SerializeField] private string playerSpawnPoint = "";     // Tên vị trí spawn của player
    [SerializeField] private string requiredPuzzleKey = "";    // PlayerPrefs key của puzzle trước
    [SerializeField] private bool requirePreviousPuzzle = false;

    [Header("UI Settings")]
    [SerializeField] private GameObject interactUI;

    private bool isPlayerInRange = false;

    private void Start()
    {
        if (interactUI != null)
            interactUI.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInRange = true;

        if (interactUI)
            interactUI.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInRange = false;

        if (interactUI)
            interactUI.SetActive(false);
    }

    private void Update()
    {
        if (!isPlayerInRange) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryEnterPuzzle();
        }
    }

    private void TryEnterPuzzle()
    {
        // Kiểm tra yêu cầu puzzle trước
        if (requirePreviousPuzzle && !string.IsNullOrEmpty(requiredPuzzleKey))
        {
            if (PlayerPrefs.GetInt(requiredPuzzleKey, 0) == 0)
            {
                Debug.Log($"[PuzzleEntry] Puzzle '{puzzleSceneName}' bị khóa. Cần hoàn thành: {requiredPuzzleKey}");
                return;
            }
        }

        Debug.Log($"[PuzzleEntry] Loading puzzle scene: {puzzleSceneName}, Spawn: {playerSpawnPoint}");

        // Load scene puzzle thông qua GameManager để tránh lỗi RawImage
        GameManager.Instance.LoadSceneWithFade(puzzleSceneName, playerSpawnPoint);
    }
}
