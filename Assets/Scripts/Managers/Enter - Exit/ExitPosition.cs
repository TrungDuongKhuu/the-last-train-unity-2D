using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(Collider2D))]
public class ExitPosition : MonoBehaviour
{
    [Header("Target Scene")]
    public string targetSceneName;
    public string targetEnterID = "Enter_Default";

    [Header("Cutscene Settings")]
    public PlayableDirector cutsceneDirector;
    public Controller player;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Nếu đang load scene → bỏ qua
        if (GameManager.Instance != null && GameManager.Instance.IsLoading)
            return;

        if (triggered) return;
        triggered = true;

        Debug.Log($"[ExitPosition] Trigger → {targetSceneName}");

        if (cutsceneDirector != null)
        {
            if (player != null)
                player.DisableControl();

            cutsceneDirector.stopped += OnCutsceneFinished;
            cutsceneDirector.Play();
        }
        else
        {
            GameManager.Instance.LoadSceneWithFade(targetSceneName, targetEnterID);
        }
    }

    private void OnCutsceneFinished(PlayableDirector director)
    {
        cutsceneDirector.stopped -= OnCutsceneFinished;
        Debug.Log("[ExitPosition] Exit Cutscene finished → Load Next Scene");

        GameManager.Instance.LoadSceneWithFade(targetSceneName, targetEnterID);
    }

    private void OnEnable()
    {
        triggered = false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider2D>().bounds.size);
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 0.4f,
            $"{targetSceneName}:{targetEnterID}",
            new GUIStyle()
            {
                fontSize = 10,
                normal = new GUIStyleState() { textColor = Color.magenta }
            }
        );
    }
#endif
}
