using System.Linq;
using Commands;
using Level;
using UI;
using UnityEngine;

public class SokobanGameManager : MonoBehaviour
{
    public static SokobanGameManager Instance { get; private set; }

    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private SokobanMovementController playerMovementController;
    [SerializeField] private string playerTag = "Player";

    public bool IsGamePaused => PauseMenuController.IsPaused;
    public bool HasNextLevel => levelLoader.HasNextLevel;

    private Target[] _targets;

    private const string PREF_CURRENT_LEVEL = "SOKOBAN_CurrentLevel";

    private void Awake()
    {
        CommandHistoryHandler.Instance.Clear();
        EnsureSingleton();
    }

    private void OnEnable()
    {
        levelLoader.OnLevelLoaded += OnLevelLoaded;
    }

    private void OnDisable()
    {
        levelLoader.OnLevelLoaded -= OnLevelLoaded;

        if (_targets != null)
        {
            foreach (var target in _targets)
                target.OnOccupied -= OnTargetOccupied;
        }
    }

    private void Start()
    {
        CommandHistoryHandler.Instance.Clear();
    }

    private bool AreAllTargetsOccupied()
    {
        return _targets.Length == 0 || _targets.All(target => target.IsOccupied);
    }

    private void OnTargetOccupied()
    {
        if (!AreAllTargetsOccupied())
            return;

        int index = levelLoader.CurrentLevelIndex;

        // Save this level as completed
        PlayerPrefs.SetInt("SOKOBAN_Level_" + index, 1);
        PlayerPrefs.Save();

        // 🔥 Nếu là level cuối → out puzzle và reset tiến độ
        if (index >= levelLoader.LevelsCount - 1)
        {
            PlayerPrefs.SetInt("Chapter1_Puzzle1_AllClear", 1);
            PlayerPrefs.SetInt(PREF_CURRENT_LEVEL, 0); // reset về level 0
            PlayerPrefs.Save();

            GameManager.Instance.LoadSceneWithFade("Office Room", "Enter_FromPuzzle1");
            return;
        }

        // 🔥 Lưu tiến độ level hiện tại + 1
        PlayerPrefs.SetInt(PREF_CURRENT_LEVEL, index + 1);
        PlayerPrefs.Save();

        // Load next level
        LoadNextLevel();
    }

    private void OnLevelLoaded()
    {
        _targets = levelLoader.GetObjectsOfType<Target>();

        foreach (var target in _targets)
            target.OnOccupied += OnTargetOccupied;

        Movable player = levelLoader.GetObjectOfTypeWithTag<Movable>(playerTag);
        playerMovementController.SetPlayer(player);
    }

    private void EnsureSingleton()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RestartLevel()
    {
        CommandHistoryHandler.Instance.Clear();
        levelLoader.RestartLevel();
    }

    public void LoadNextLevel()
    {
        CommandHistoryHandler.Instance.Clear();
        levelLoader.LoadNextLevel();
    }

    // Compatibility for old UI scripts
    public void PauseGame() { }
    public void ResumeGame() { }
}
