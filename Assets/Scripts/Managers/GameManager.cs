using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

/// <summary>
/// Quản lý chuyển scene + fade tròn.
/// Giữ nguyên trong _Systems, load các scene gameplay Additive.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("System Scene")]
    [Tooltip("Tên scene hệ thống (không unload scene này).")]
    public string systemsSceneName = "_Systems";

    [Header("Fade Settings")]
    public RawImage fadeImage;
    [Range(0.1f, 3f)] public float fadeSpeed = 0.8f;

    [Header("Debug State")]
    [SerializeField] private string currentGameplayScene = "";
    [SerializeField] private bool isLoading = false;

    private PlayerManager currentPlayer;
    private Material fadeMat;

    private bool isFading = false;
    public bool IsLoading => isLoading;

    private string previousScene = "";
    public string PreviousScene => previousScene;


    // ================================================================
    // LIFECYCLE
    // ================================================================
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (fadeImage != null)
        {
            fadeMat = fadeImage.material;
            fadeImage.enabled = false;
        }   

        if (!HasAnyGameplaySceneLoaded())
        {
            StartCoroutine(LoadFirstSceneAdditive("BedroomScene", "Enter_Default")); //BedroomScene
        }
    }


    // ================================================================
    // PUBLIC API
    // ================================================================
    public void RegisterPlayer(PlayerManager player)
    {
        if (player == null) return;
        if (player.isMainPlayer)
            currentPlayer = player;
    }

    public void LoadSceneWithFade(string nextScene, string enterID = "Enter_Default")
    {
        if (isLoading)
        {
            Debug.LogWarning($"[GameManager] LoadSceneWithFade ignored because isLoading = true ({nextScene})");
            return;
        }

        StartCoroutine(SwapSceneRoutine(nextScene, enterID));
    }


    // ================================================================
    // FIRST LOAD
    // ================================================================
    private IEnumerator LoadFirstSceneAdditive(string firstScene, string enterID)
    {
        isLoading = true;
        Debug.Log($"[GameManager] Loading first scene: {firstScene}");

        yield return SceneManager.LoadSceneAsync(firstScene, LoadSceneMode.Additive);
        currentGameplayScene = firstScene;
        SetActiveTo(currentGameplayScene);

        yield return new WaitUntil(() => currentPlayer != null);

        MovePlayerToGameplayScene();
        TeleportPlayerToSpawn(enterID);

        yield return FadeIn(fadeSpeed);
        isLoading = false;
    }


    // ================================================================
    // SCENE SWAP
    // ================================================================
    private IEnumerator SwapSceneRoutine(string nextScene, string enterID)
    {
        previousScene = currentGameplayScene;
        isLoading = true;

        Debug.Log($"[GameManager] Starting swap: {previousScene} -> {nextScene}");

        yield return FadeOut(fadeSpeed);
        yield return new WaitForSeconds(0.25f);

        AsyncOperation unloadOp = null;

        if (!string.IsNullOrEmpty(currentGameplayScene) &&
            currentGameplayScene != systemsSceneName)
        {
            var oldScene = SceneManager.GetSceneByName(currentGameplayScene);
            if (oldScene.isLoaded)
            {
                Debug.Log($"[GameManager] Unloading scene: {oldScene.name}");
                unloadOp = SceneManager.UnloadSceneAsync(oldScene);
            }
        }

        if (unloadOp != null)
            yield return unloadOp;

        yield return SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Additive);
        currentGameplayScene = nextScene;
        SetActiveTo(currentGameplayScene);

        float waitTime = 0f;
        while (currentPlayer == null && waitTime < 2f)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }

        bool hasEntryCutscene = SceneHasEntryCutscene(enterID, previousScene);

        if (currentPlayer != null)
        {
            MovePlayerToGameplayScene();

            if (!hasEntryCutscene)
                TeleportPlayerToSpawn(enterID);
        }

        yield return new WaitForSeconds(0.25f);
        yield return FadeIn(fadeSpeed);

        if (currentPlayer != null && !hasEntryCutscene)
        {
            var ctrl = currentPlayer.GetComponent<Controller>();
            if (ctrl != null) ctrl.EnableControl();

            var pm = currentPlayer.GetComponent<PlayerMovement>();
            if (pm != null) pm.EnableControl();
        }

        isLoading = false;
        Debug.Log($"[GameManager] Scene swap complete: {previousScene} -> {currentGameplayScene}");

        StartCoroutine(DelayRestoreUI(currentGameplayScene));
    }


    // ================================================================
    // FADE
    // ================================================================
    private IEnumerator FadeOut(float duration)
    {
        if (fadeMat == null) yield break;

        fadeImage.enabled = true;
        isFading = true;

        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            fadeMat.SetFloat("_Radius", Mathf.Lerp(1f, -0.1f, t / duration));
            yield return null;
        }
        fadeMat.SetFloat("_Radius", -0.1f);
    }

    private IEnumerator FadeIn(float duration)
    {
        if (fadeMat == null) yield break;

        isFading = true;
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            fadeMat.SetFloat("_Radius", Mathf.Lerp(-0.1f, 1f, t / duration));
            yield return null;
        }

        fadeMat.SetFloat("_Radius", 1f);
        fadeImage.enabled = false;
        isFading = false;
    }


    // ================================================================
    // HELPERS
    // ================================================================
    private bool HasAnyGameplaySceneLoaded()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name != systemsSceneName)
                return true;
        }
        return false;
    }

    private void SetActiveTo(string sceneName)
    {
        var sc = SceneManager.GetSceneByName(sceneName);
        if (sc.IsValid()) SceneManager.SetActiveScene(sc);
    }

    private bool SceneHasEntryCutscene(string enterID, string fromScene)
    {
        var roots = SceneManager.GetActiveScene().GetRootGameObjects();

        return roots
            .SelectMany(r => r.GetComponentsInChildren<EnterPosition>(true))
            .Any(e =>
                e.cutsceneDirector != null &&
                (string.IsNullOrEmpty(e.triggerFromScene) || e.triggerFromScene == fromScene) &&
                (string.IsNullOrEmpty(enterID) || e.enterID == enterID)
            );
    }

    private void TeleportPlayerToSpawn(string enterID)
    {
        if (currentPlayer == null) return;

        var roots = SceneManager.GetActiveScene().GetRootGameObjects();
        var spawn = roots
            .SelectMany(r => r.GetComponentsInChildren<EnterPosition>(true))
            .FirstOrDefault(s => s.enterID == enterID)
            ?? roots.SelectMany(r => r.GetComponentsInChildren<EnterPosition>(true)).FirstOrDefault();

        if (spawn != null)
        {
            currentPlayer.transform.position = spawn.transform.position;
            Debug.Log($"[GameManager] Teleported Player to {enterID}");
        }
    }

    private void MovePlayerToGameplayScene()
    {
        if (currentPlayer == null) return;

        var sc = SceneManager.GetSceneByName(currentGameplayScene);
        if (sc.IsValid())
            SceneManager.MoveGameObjectToScene(currentPlayer.gameObject, sc);
    }


    // ================================================================
    // ENDING
    // ================================================================
    public void LoadEnding()
    {
        int truth = TruthfulnessManager.Instance.GetTruth();

        if (truth < 30)
            SceneManager.LoadScene("Bad Ending");
        else if (truth < 70)
            SceneManager.LoadScene("Neutral Ending");
        else
            SceneManager.LoadScene("Good Ending");
    }


    // ================================================================
    // GLOBAL UI RESTORE
    // ================================================================
    private IEnumerator DelayRestoreUI(string sceneName)
    {
        // Đợi 1 frame — quan trọng!
        yield return null;

        RestoreMenuAfterSceneLoad(sceneName);
    }

    private void RestoreMenuAfterSceneLoad(string sceneName)
    {
        if (sceneName == "Chapter 1 Puzzle 1" || sceneName == "Chapter 1 Puzzle 2")
            return;

        // Menu HUD
        MenuManager.instance?.ShowHUD(true);

        // Inventory UI — auto find if Instance is null
        if (InventoryUI.Instance == null)
        {
            InventoryUI ui = Object.FindFirstObjectByType<InventoryUI>();
            if (ui != null)
                InventoryUI.Instance = ui;
        }

        if (InventoryUI.Instance != null)
            InventoryUI.Instance.gameObject.SetActive(true);
    }


}
