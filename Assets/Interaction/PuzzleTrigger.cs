using UnityEngine;

/// <summary>
/// Gắn vào một đối tượng tương tác (nút bấm, cần gạt) để gửi tín hiệu đến PuzzleManager.
/// </summary>
public class PuzzleTrigger : MonoBehaviour, IInteractable
{
    [Header("Puzzle Link")]
    [Tooltip("Kéo PuzzleManager mà đối tượng này thuộc về vào đây.")]
    [SerializeField] private PuzzleManager puzzleManager;

    [Tooltip("ID của 'mảnh ghép' mà đối tượng này đại diện. Phải khớp với ID trong PuzzleManager.")]
    [SerializeField] private string pieceId;

    [Header("Interaction")]
    [Tooltip("Trạng thái sẽ được gửi đến PuzzleManager khi tương tác.")]
    [SerializeField] private bool stateToSend = true;
    [SerializeField] private string interactionPrompt = "Tương tác";

    [Header("UI Indicator")]
    [Tooltip("Prefab của dấu chấm than hoặc icon tương tác.")]
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private Vector3 indicatorOffset = new Vector3(0, 1.2f, 0);

    private GameObject indicatorInstance;

    public void Interact()
    {
        if (puzzleManager == null)
        {
            Debug.LogError("PuzzleManager chưa được gán trong PuzzleTrigger!", this);
            return;
        }

        // Gửi trạng thái đến PuzzleManager
        puzzleManager.UpdatePieceState(pieceId, stateToSend);

        // Tùy chọn: Bạn có thể thêm logic để không cho tương tác lại, ví dụ:
        // this.enabled = false; // Vô hiệu hóa script này
        // GetComponent<Collider2D>().enabled = false; // Vô hiệu hóa trigger
    }

    #region Interface Implementation
    public string GetInteractionText()
    {
        return interactionPrompt;
    }

    public void ShowIndicator()
    {
        if (indicatorPrefab != null && indicatorInstance == null)
        {
            Canvas mainCanvas = FindFirstObjectByType<Canvas>();
            if (mainCanvas != null)
                indicatorInstance = Instantiate(indicatorPrefab, mainCanvas.transform);
        }
        if (indicatorInstance != null)
        {
            indicatorInstance.SetActive(true);
            UpdateIndicatorPosition();
        }
    }

    public void HideIndicator()
    {
        if (indicatorInstance != null)
            indicatorInstance.SetActive(false);
    }
    #endregion

    private void UpdateIndicatorPosition()
    {
        if (indicatorInstance != null && indicatorInstance.activeSelf && Camera.main != null)
        {
            indicatorInstance.transform.position = Camera.main.WorldToScreenPoint(transform.position + indicatorOffset);
        }
    }

    private void Update()
    {
        UpdateIndicatorPosition();
    }

    private void OnDestroy()
    {
        if (indicatorInstance != null)
            Destroy(indicatorInstance);
    }
}
