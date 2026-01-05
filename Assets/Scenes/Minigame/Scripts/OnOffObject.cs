using UnityEngine;
using UnityEngine.Events;

public class OnOffObject : MonoBehaviour
{
    public enum CheckMode { ALL, ANY }

    [Header("Cấu hình nhiều PlayerPrefs")]
    public string[] flagKeys;
    public int targetValue = 1;
    public CheckMode checkMode = CheckMode.ALL;
    public bool invertCondition = false;

    [Header("Object khi TRUE")]
    public GameObject[] activateOnTrue;
    public GameObject[] deactivateOnTrue;
    public UnityEvent onConditionMet;

    [Header("Cutscene chỉ chạy 1 lần")]
    public bool playCutsceneOnce = true;
    public string cutscenePlayedKey = "CutscenePlayedKey";

    [Header("Object khi FALSE")]
    public GameObject[] activateOnFalse;
    public GameObject[] deactivateOnFalse;
    public UnityEvent onConditionNotMet;

    public bool checkOnStart = true;

    private void OnEnable()
    {
        // Lắng nghe khi bất kỳ PlayerPrefs nào thay đổi
        PlayerPrefsMonitor.OnChanged += OnPrefChanged;
    }

    private void OnDisable()
    {
        PlayerPrefsMonitor.OnChanged -= OnPrefChanged;
    }

    private void Start()
    {
        if (checkOnStart)
            CheckState();
    }

    private void OnPrefChanged(string changedKey)
    {
        // Chỉ khi key liên quan đến script này mới kiểm tra lại
        foreach (string key in flagKeys)
        {
            if (key == changedKey)
            {
                CheckState();
                return;
            }
        }
    }

    // =================== CHECK STATE =======================
    public void CheckState()
    {
        bool result = EvaluateCondition();

        if (invertCondition)
            result = !result;

        if (result)
            HandleTrue();
        else
            HandleFalse();
    }

    private bool EvaluateCondition()
    {
        if (flagKeys == null || flagKeys.Length == 0)
            return false;

        int matchCount = 0;

        foreach (var key in flagKeys)
        {
            int value = PlayerPrefs.GetInt(key, 0);

            if (value == targetValue)
                matchCount++;

            if (checkMode == CheckMode.ANY && value == targetValue)
                return true;
        }

        return checkMode == CheckMode.ALL && matchCount == flagKeys.Length;
    }

    private void HandleTrue()
    {
        ActivateObjects(activateOnTrue, true);
        ActivateObjects(deactivateOnTrue, false);

        if (playCutsceneOnce)
        {
            if (PlayerPrefs.GetInt(cutscenePlayedKey, 0) == 0)
            {
                onConditionMet?.Invoke();
                PlayerPrefs.SetInt(cutscenePlayedKey, 1);
                PlayerPrefs.Save();
            }
        }
        else
        {
            onConditionMet?.Invoke();
        }
    }

    private void HandleFalse()
    {
        ActivateObjects(activateOnFalse, true);
        ActivateObjects(deactivateOnFalse, false);

        onConditionNotMet?.Invoke();
    }

    private void ActivateObjects(GameObject[] objs, bool state)
    {
        if (objs == null) return;

        foreach (GameObject obj in objs)
            if (obj != null)
                obj.SetActive(state);
    }
}
