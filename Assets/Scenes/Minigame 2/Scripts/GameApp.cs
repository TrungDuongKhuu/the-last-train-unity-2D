using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameApp : Singleton<GameApp>
{
    public bool TileMovementEnabled { get; set; } = false;
    public double SecondsSinceStart { get; set; } = 0;
    public int TotalTilesInCorrectPosition { get; set; } = 0;

    [SerializeField]
    List<string> jigsawImageNames = new List<string>();

    int imageIndex = 0;   // index của hình hiện tại

    // Đếm số lần đã hoàn thành Puzzle 2
    public int puzzle2CompletedCount = 0;

    // Tổng số hình cần hoàn thành
    public int puzzle2TargetCount = 5;


    /// <summary>
    /// Trả về TÊN HÌNH HIỆN TẠI cho Puzzle 2.
    /// KHÔNG TĂNG INDEX – để out/in scene vẫn giữ cùng 1 hình.
    /// </summary>
    public string GetJigsawImageName()
    {
        if (jigsawImageNames == null || jigsawImageNames.Count == 0)
        {
            Debug.LogError("[GameApp] jigsawImageNames rỗng.");
            return string.Empty;
        }

        // đảm bảo index nằm trong khoảng hợp lệ
        if (imageIndex < 0 || imageIndex >= jigsawImageNames.Count)
        {
            imageIndex = 0;
        }

        string imageName = jigsawImageNames[imageIndex];
        return imageName;
    }

    /// <summary>
    /// Gọi khi người chơi hoàn thành MỘT HÌNH puzzle 2.
    /// Tăng biến đếm + nếu chưa đủ thì CHUYỂN sang hình kế tiếp.
    /// </summary>
    public void OnPuzzle2Finished()
    {
        puzzle2CompletedCount++;
        Debug.Log($"Puzzle2 completed: {puzzle2CompletedCount}/{puzzle2TargetCount}");

        if (puzzle2CompletedCount >= puzzle2TargetCount)
        {
            PlayerPrefs.SetInt("Chapter1_Puzzle2_AllClear", 1);
            PlayerPrefs.Save();
            Debug.Log("Chapter1_Puzzle2_AllClear = 1");
        }

        // ⭐ CHỈ SAU KHI HOÀN THÀNH MỚI CHUYỂN SANG HÌNH KẾ TIẾP
        if (jigsawImageNames != null && jigsawImageNames.Count > 0)
        {
            // Nếu muốn lặp vòng qua hình 1 lại thì dùng:
            // imageIndex = (imageIndex + 1) % jigsawImageNames.Count;

            // Nếu muốn đi 1 chiều rồi dừng ở hình cuối (phù hợp cốt truyện) thì dùng:
            imageIndex = Mathf.Clamp(imageIndex + 1, 0, jigsawImageNames.Count - 1);
        }
    }
}
