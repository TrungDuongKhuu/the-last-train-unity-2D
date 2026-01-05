using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class PaperManager : MonoBehaviour
{
    public static PaperManager Instance { get; private set; }

    [Header("Cài đặt số mảnh giấy")]
    public int totalPieces = 6;

    [Header("UI hiển thị")]
    [SerializeField] private TextMeshProUGUI counterText;

    [Header("Lưu tiến độ tổng")]
    [SerializeField] private string playerPrefsKeyAllCollected = "Chapter1_Puzzle2_AllClear";

    [Header("Sự kiện khi nhặt đủ mảnh")]
    public UnityEvent onAllPiecesCollected;

    public int CollectedPieces { get; private set; }

    private bool allCollectedInvoked = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Khôi phục số mảnh đã nhặt dựa vào PlayerPrefs từng mảnh
        RebuildCollectedFromSavedPieces();

        UpdateUI();

        if (allCollectedInvoked)
        {
            onAllPiecesCollected?.Invoke();
        }
    }

    // Đếm lại tất cả các mảnh đã nhặt dựa trên PlayerPrefs
    private void RebuildCollectedFromSavedPieces()
    {
        CollectedPieces = 0;

        // Tìm tất cả mảnh giấy trong scene (kể cả inactive)
        var pieces = FindObjectsByType<PaperPiecePickup>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (var piece in pieces)
        {
            if (piece == null) continue;

            string key = $"PaperPiece_{piece.paperID}_Collected";

            if (PlayerPrefs.GetInt(key, 0) == 1)
            {
                CollectedPieces++;
            }
        }

        // Nếu trước đó puzzle đã được đánh dấu "Clear"
        if (PlayerPrefs.GetInt(playerPrefsKeyAllCollected, 0) == 1 || CollectedPieces >= totalPieces)
        {
            CollectedPieces = totalPieces;
            allCollectedInvoked = true;

            PlayerPrefs.SetInt(playerPrefsKeyAllCollected, 1);
            PlayerPrefs.Save();
        }
        else
        {
            allCollectedInvoked = false;
        }
    }

    public void CollectPiece()
    {
        if (CollectedPieces >= totalPieces) return;

        CollectedPieces++;
        UpdateUI();

        if (CollectedPieces >= totalPieces && !allCollectedInvoked)
        {
            allCollectedInvoked = true;

            PlayerPrefs.SetInt(playerPrefsKeyAllCollected, 1);
            PlayerPrefs.Save();

            onAllPiecesCollected?.Invoke();
        }
    }

    private void UpdateUI()
    {
        if (counterText != null)
        {
            counterText.text = $"{CollectedPieces} / {totalPieces}";
        }
    }

    [ContextMenu("Reset Paper Progress")]
    public void ResetProgress()
    {
        CollectedPieces = 0;
        allCollectedInvoked = false;

        PlayerPrefs.SetInt(playerPrefsKeyAllCollected, 0);

        // Reset từng mảnh giấy
        var pieces = FindObjectsByType<PaperPiecePickup>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (var piece in pieces)
        {
            if (piece == null) continue;
            PlayerPrefs.SetInt($"PaperPiece_{piece.paperID}_Collected", 0);
        }

        PlayerPrefs.Save();
        UpdateUI();
    }
}
