using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

// Lớp chứa thông tin về một "mảnh ghép" của câu đố
[System.Serializable]
public class PuzzlePiece
{
    [Tooltip("Tên định danh cho mảnh ghép, ví dụ: 'Nút bấm màu đỏ', 'Cần gạt bên trái'.")]
    public string pieceId;

    [Tooltip("Trạng thái yêu cầu của mảnh ghép này để được coi là 'đúng'.")]
    public bool requiredState = true;

    [Tooltip("Trạng thái hiện tại của mảnh ghép. Sẽ được cập nhật bởi PuzzleTrigger.")]
    [HideInInspector] public bool currentState = false;
}

/// <summary>
/// Quản lý trạng thái của một câu đố phức tạp.
/// Kích hoạt một sự kiện khi tất cả các "mảnh ghép" (Puzzle Pieces) đều đạt trạng thái yêu cầu.
/// </summary>
public class PuzzleManager : MonoBehaviour
{
    [Header("Puzzle Configuration")]
    [Tooltip("Danh sách tất cả các thành phần của câu đố.")]
    [SerializeField] private List<PuzzlePiece> puzzlePieces = new List<PuzzlePiece>();

    [Header("Events")]
    [Tooltip("Sự kiện sẽ được kích hoạt một lần duy nhất khi câu đố được giải thành công.")]
    public UnityEvent onPuzzleSolved;

    private bool isSolved = false; // Cờ để đảm bảo sự kiện chỉ được gọi một lần

    /// <summary>
    /// Được gọi bởi các PuzzleTrigger để cập nhật trạng thái của một mảnh ghép.
    /// </summary>
    /// <param name="pieceId">ID của mảnh ghép cần cập nhật.</param>
    /// <param name="newState">Trạng thái mới để gán.</param>
    public void UpdatePieceState(string pieceId, bool newState)
    {
        if (isSolved) return; // Nếu câu đố đã được giải, không làm gì cả

        // Tìm mảnh ghép trong danh sách
        PuzzlePiece piece = puzzlePieces.FirstOrDefault(p => p.pieceId == pieceId);

        if (piece != null)
        {
            piece.currentState = newState;
            Debug.Log($"Trạng thái của mảnh ghép '{pieceId}' đã được cập nhật thành: {newState}");
            CheckForCompletion();
        }
        else
        {
            Debug.LogWarning($"Không tìm thấy mảnh ghép nào với ID: '{pieceId}' trong PuzzleManager này.", this);
        }
    }

    /// <summary>
    /// Kiểm tra xem tất cả các mảnh ghép đã đạt trạng thái yêu cầu hay chưa.
    /// </summary>
    private void CheckForCompletion()
    {
        // Dùng Linq để kiểm tra nếu TẤT CẢ các mảnh ghép đều thỏa mãn điều kiện (currentState == requiredState)
        bool allPiecesCorrect = puzzlePieces.All(p => p.currentState == p.requiredState);

        if (allPiecesCorrect && !isSolved)
        {
            isSolved = true;
            Debug.Log("Chúc mừng! Câu đố đã được giải!", this);
            onPuzzleSolved.Invoke();
        }
    }
}
