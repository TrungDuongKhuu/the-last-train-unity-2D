using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using System.IO;

/// <summary>
/// Singleton quản lý InputActions và rebinds.
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("Input Actions Asset")]
    public InputActionAsset actionsAsset;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load các rebinds lưu từ trước
        LoadRebinds();

        // Enable tất cả action map mặc định
        foreach (var map in actionsAsset.actionMaps)
            map.Enable();
    }

    /// <summary>
    /// Lấy InputAction theo tên
    /// </summary>
    public InputAction GetAction(string actionName)
    {
        foreach (var map in actionsAsset.actionMaps)
        {
            var action = map.FindAction(actionName, true);
            if (action != null) return action;
        }
        Debug.LogWarning($"Action '{actionName}' không tìm thấy trong InputManager.");
        return null;
    }

    /// <summary>
    /// Lưu rebinds của tất cả action vào PlayerPrefs
    /// </summary>
    public void SaveRebinds()
    {
        foreach (var map in actionsAsset.actionMaps)
        {
            foreach (var action in map.actions)
            {
                string rebinds = action.SaveBindingOverridesAsJson();
                if (!string.IsNullOrEmpty(rebinds))
                    PlayerPrefs.SetString(action.name, rebinds);
            }
        }
        PlayerPrefs.Save();
        Debug.Log("InputManager: Saved rebinds.");
    }

    /// <summary>
    /// Load rebinds từ PlayerPrefs
    /// </summary>
    public void LoadRebinds()
    {
        foreach (var map in actionsAsset.actionMaps)
        {
            foreach (var action in map.actions)
            {
                if (PlayerPrefs.HasKey(action.name))
                {
                    string rebinds = PlayerPrefs.GetString(action.name);
                    if (!string.IsNullOrEmpty(rebinds))
                    {
                        action.LoadBindingOverridesFromJson(rebinds);
                        Debug.Log($"InputManager: Loaded rebinds for {action.name}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Reset tất cả rebind về mặc định
    /// </summary>
    public void ResetRebinds()
    {
        foreach (var map in actionsAsset.actionMaps)
        {
            foreach (var action in map.actions)
            {
                action.RemoveAllBindingOverrides();
            }
        }
        PlayerPrefs.DeleteAll();
        Debug.Log("InputManager: Reset all rebinds.");
    }
}
