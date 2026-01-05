using UnityEngine;
using UnityEngine.Playables;

public class EnterPosition : MonoBehaviour
{
    [Tooltip("ID định danh vị trí xuất hiện của người chơi trong Scene này.")]
    public string enterID = "Enter_Default";

    [Header("Cutscene Settings (Tùy chọn)")]
    public PlayableDirector cutsceneDirector;

    [Tooltip("Player trong scene (nếu có). GameManager sẽ chuyển player sang scene này sau khi load.")]
    public Controller player;

    [Tooltip("Tên Scene MÀ TỪ ĐÓ người chơi đi tới thì mới chiếu cutscene này.")]
    public string triggerFromScene = "";

    private bool cutscenePlayed = false;

    private void Start()
    {
        if (cutsceneDirector == null) return;
        if (cutscenePlayed) return;

        string fromScene = GameManager.Instance != null ? GameManager.Instance.PreviousScene : "";

        // Nếu không khớp triggerFromScene thì bỏ qua
        if (!string.IsNullOrEmpty(triggerFromScene) &&
            fromScene != triggerFromScene)
            return;

        PlayEnterCutscene();
    }

    private void PlayEnterCutscene()
    {
        cutscenePlayed = true;

        if (player != null)
        {
            // Ẩn nhân vật trước khi timeline chạy
            player.gameObject.SetActive(false);
            player.DisableControl();
        }

        Debug.Log($"[EnterPosition] Play Entry Cutscene at {enterID}");

        cutsceneDirector.stopped += OnCutsceneFinished;
        cutsceneDirector.Play();
    }

    private void OnCutsceneFinished(PlayableDirector director)
    {
        cutsceneDirector.stopped -= OnCutsceneFinished;

        if (player != null)
        {
            player.gameObject.SetActive(true);
            player.EnableControl();

            var sr = player.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                var c = sr.color;
                c.a = 1f;
                sr.color = c;
            }
        }

        Debug.Log("[EnterPosition] Entry Cutscene finished → Player appear");
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 0.4f,
            $"EnterID: {enterID} (From: {triggerFromScene})",
            new GUIStyle()
            {
                fontSize = 10,
                normal = new GUIStyleState() { textColor = Color.yellow }
            }
        );
    }
#endif
}
