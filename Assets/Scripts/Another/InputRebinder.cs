using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputRebinder : MonoBehaviour
{
    public InputActionReference action; // InputAction muốn rebind
    public Button rebindButton;
    public TextMeshProUGUI keyText;

    void Start()
    {
        UpdateUI();
        rebindButton.onClick.AddListener(StartRebind);
    }

    void UpdateUI()
    {
        //if (action.action.bindings.Count > 0)
            keyText.text = action.action.bindings[0].effectivePath;
    }

    public void StartRebind()
    {
        keyText.text = "Press new key...";
        action.action.PerformInteractiveRebinding()
            .WithControlsExcluding("<Mouse>/position")
            .OnComplete(operation =>
            {
                operation.Dispose();
                UpdateUI();
                SaveBinding();
            })
            .Start();
    }

    void SaveBinding()
    {
        string rebinds = action.action.actionMap.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebinds);
        PlayerPrefs.Save();
    }

    public void LoadBinding()
    {
        string rebinds = PlayerPrefs.GetString("rebinds", "");
        if (!string.IsNullOrEmpty(rebinds))
        {
            action.action.actionMap.LoadBindingOverridesFromJson(rebinds);
            UpdateUI();
        }
    }
}
