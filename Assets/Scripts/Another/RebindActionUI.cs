using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class RebindButton : MonoBehaviour
{
    public string actionName;
    public int bindingIndex = 0;

    public TextMeshProUGUI currentKeyText;

    private InputAction action;

    void Start()
    {
        action = InputManager.Instance.GetAction(actionName);

        UpdateUI();
    }

    void UpdateUI()
    {
        currentKeyText.text = InputControlPath.ToHumanReadableString(
            action.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice
        );
    }

    public void StartRebind()
    {
        gameObject.GetComponent<Button>().interactable = false;
        currentKeyText.text = "Press a key...";

        action.PerformInteractiveRebinding(bindingIndex)
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(operation =>
            {
                operation.Dispose();
                InputManager.Instance.SaveRebinds();
                UpdateUI();
                gameObject.GetComponent<Button>().interactable = true;
            })
            .Start();
    }
}
