using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Quản lý rebinding cho các InputAction:
/// - Move Vector2 Composite (Up, Down, Left, Right)
/// - Jump (single binding)
/// - Attack (single binding)
/// Gán script vào 1 Panel chứa các Button/Text.
/// </summary>
public class Vector2RebindUI : MonoBehaviour
{
    [System.Serializable]
    public class ActionBindingUI
    {
        [Header("Action Info")]
        public string actionName;        // Tên action trong Input Actions asset
        public int bindingIndex;         // Index của binding (0-3 cho composite, 0 cho single)

        [Header("UI References")]
        public TextMeshProUGUI keyText;  // Text hiển thị key hiện tại
        public Button rebindButton;      // Button để rebinding
    }

    [Header("Bindings UI List")]
    [Tooltip("Danh sách các entry cho Move Composite + Jump + Attack")]
    public List<ActionBindingUI> bindingsUI;

    //private void Awake()
    //{
    //    // Load rebinds từ PlayerPrefs trước khi cập nhật UI
    //    InputManager.Instance.LoadRebinds();
    //}

    //private void Start()
    //{

    //    foreach (var ui in bindingsUI)
    //    {
    //        InputAction action = InputManager.Instance.GetAction(ui.actionName);
    //        if (action == null)
    //        {
    //            Debug.LogError($"Action '{ui.actionName}' không tìm thấy trong InputManager.");
    //            continue;
    //        }

    //        // Cập nhật UI hiển thị key hiện tại
    //        UpdateUI(ui, action);

    //        // Thêm listener cho button
    //        ui.rebindButton.onClick.AddListener(() => StartRebind(ui, action));
    //    }
    //}

    private IEnumerator Start()
    {
        // Đợi InputManager Awake xong
        yield return null;

        if (InputManager.Instance == null)
        {
            Debug.LogError("InputManager chưa sẵn sàng!");
            yield break;
        }

        InputManager.Instance.LoadRebinds();


        foreach (var ui in bindingsUI)
        {
            InputAction action = InputManager.Instance.GetAction(ui.actionName);
            if (action == null)
            {
                Debug.LogError($"Action '{ui.actionName}' không tìm thấy trong InputManager.");
                continue;
            }

            UpdateUI(ui, action);
            ui.rebindButton.onClick.AddListener(() => StartRebind(ui, action));
        }
    }


    /// <summary>
    /// Cập nhật Text hiển thị key hiện tại
    /// </summary>
    void UpdateUI(ActionBindingUI ui, InputAction action)
    {
        if (ui.keyText == null) return;

        var binding = action.bindings[ui.bindingIndex];

        // Lấy effectivePath chính xác (cả composite và single binding)
        ui.keyText.text = InputControlPath.ToHumanReadableString(
            binding.effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice
        );
    }

    /// <summary>
    /// Bắt đầu interactive rebinding cho 1 binding
    /// </summary>
    void StartRebind(ActionBindingUI ui, InputAction action)
    {
        if (ui.rebindButton == null || ui.keyText == null) return;

        ui.rebindButton.interactable = false;
        ui.keyText.text = "Press a key...";

        // Tắt action trước khi rebinding
        bool wasEnabled = action.enabled;
        if (wasEnabled) action.Disable();

        // Bắt đầu rebinding
        action.PerformInteractiveRebinding(ui.bindingIndex)
            .WithCancelingThrough("<Keyboard>/escape") // Nhấn Esc để hủy
            .OnComplete(op =>
            {
                op.Dispose();

                // Lưu binding
                InputManager.Instance.SaveRebinds();

                // Cập nhật UI
                UpdateUI(ui, action);

                ui.rebindButton.interactable = true;

                // Enable lại action nếu trước đó đang enable
                if (wasEnabled) action.Enable();
            })
            .Start();
    }
}
