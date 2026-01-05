using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events; // Thêm dòng này

/// <summary>
/// Quản lý việc tương tác của người chơi với các đối tượng IInteractable.
/// Tự động tìm đối tượng gần nhất, hiển thị UI và xử lý input.
/// UI tương tác được tạo ra từ một Prefab khi game bắt đầu.
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Kéo Prefab của TextMeshPro UI vào đây.")]
    [SerializeField] private GameObject interactionTextPrefab;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    [Header("Interaction Text Position")]
    [Tooltip("Khoảng cách của text so với tâm của nhân vật. VD: (0, 1.5, 0) để hiện trên đầu.")]
    [SerializeField] private Vector3 textOffset = new Vector3(0, 1.5f, 0);

    [Header("Events")]
    [Tooltip("Sự kiện này được kích hoạt khi người chơi nhấn phím tương tác (E).")]
    public UnityEvent OnInteractKeyPressed;

    // Biến này sẽ lưu trữ đối tượng UI được tạo ra từ Prefab
    private TextMeshProUGUI interactionTextInstance;

    // Danh sách các đối tượng có thể tương tác trong tầm
    private List<IInteractable> interactablesInRange = new List<IInteractable>();
    private IInteractable currentInteractable;
    private Camera mainCamera;
    private Controller playerController;

    private void Start()
    {
        mainCamera = Camera.main;
        playerController = GetComponent<Controller>();
        InstantiateInteractionUI();
    }

    private void Update()
    {
        FindClosestInteractable();
        HandleInteractionInput();
        HandleItemDropInput(); // Thêm dòng này
    }

    // Sử dụng LateUpdate để cập nhật vị trí UI, tránh bị giật khi nhân vật di chuyển
    private void LateUpdate()
    {
        UpdateInteractionTextPosition();
    }
    
    /// <summary>
    /// Tạo ra UI tương tác từ Prefab và gắn nó vào Canvas.
    /// </summary>
    private void InstantiateInteractionUI()
    {
        if (interactionTextPrefab == null)
        {
            Debug.LogWarning("Chưa gán Interaction Text Prefab cho PlayerInteractor!");
            return;
        }

        Canvas mainCanvas = Object.FindFirstObjectByType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("Không tìm thấy Canvas nào trong Scene! UI tương tác không thể được tạo.");
            return;
        }

        // Tạo một instance của Prefab và đặt nó làm con của Canvas
        GameObject textObject = Instantiate(interactionTextPrefab, mainCanvas.transform);
        interactionTextInstance = textObject.GetComponent<TextMeshProUGUI>();

        if (interactionTextInstance == null)
        {
            Debug.LogError("Prefab được gán không chứa component TextMeshProUGUI!");
            Destroy(textObject); // Xóa đối tượng rác vừa tạo ra
            return;
        }
        
        // Mặc định ẩn UI đi
        interactionTextInstance.gameObject.SetActive(false);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null && !interactablesInRange.Contains(interactable))
        {
            interactablesInRange.Add(interactable);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null && interactablesInRange.Contains(interactable))
        {
            interactablesInRange.Remove(interactable);
        }
    }

    private void FindClosestInteractable()
    {
        IInteractable closest = null;
        
        // Dọn dẹp danh sách, loại bỏ các đối tượng đã bị phá hủy (null)
        interactablesInRange.RemoveAll(item => item == null || (item as MonoBehaviour) == null);

        if (interactablesInRange.Count > 0)
        {
            // Tìm đối tượng gần nhất bằng Linq (ngắn gọn và hiệu quả)
            closest = interactablesInRange
                .OrderBy(item => Vector2.Distance(transform.position, (item as MonoBehaviour).transform.position))
                .FirstOrDefault();
        }

        // Chỉ cập nhật nếu đối tượng tương tác mục tiêu đã thay đổi
        if (closest != currentInteractable)
        {
            // Ẩn indicator của đối tượng CŨ trước khi chuyển sang đối tượng mới
            if (currentInteractable != null)
            {
                currentInteractable.HideIndicator();
            }

            // Cập nhật sang đối tượng MỚI
            currentInteractable = closest;

            // Hiển thị thông tin cho đối tượng MỚI (nếu nó tồn tại)
            if (currentInteractable != null)
            {
                ShowInteractionPrompt(currentInteractable.GetInteractionText());
                currentInteractable.ShowIndicator();
            }
            else // Nếu không còn đối tượng nào trong tầm
            {
                ClearInteractionPrompt();
            }
        }
        
        // Cập nhật biến nearInteractableItem trong Controller
        if (playerController != null)
        {
            // Kiểm tra xem có vật phẩm ItemPickup nào gần không
            bool nearItemPickup = currentInteractable != null && currentInteractable is ItemPickup;
            playerController.nearInteractableItem = nearItemPickup;
        }
    }
    
    private void HandleInteractionInput()
    {
        if (Input.GetKeyDown(interactionKey)) // Phím E được nhấn
        {
            // ƯU TIÊN 1: Tương tác với thế giới
            if (currentInteractable != null)
            {
                // Kích hoạt sự kiện (nếu có)
                OnInteractKeyPressed?.Invoke();
                // Tương tác với đối tượng gần nhất
                currentInteractable.Interact();
            }
            // ƯU TIÊN 2: Sử dụng vật phẩm trong túi đồ
            else
            {
                // Lấy vật phẩm đang được chọn
                ItemData selectedItem = InventoryManager.instance.GetSelectedItem();
                if (selectedItem != null)
                {
                    // Sử dụng vật phẩm
                    Debug.Log($"Sử dụng vật phẩm từ túi đồ: {selectedItem.itemName}");
                    selectedItem.Use(playerController);
                }
            }
        }
    }

    private void HandleItemDropInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            InventoryManager.instance.DropSelectedItem();
        }
    }

    /// <summary>
    /// Cập nhật vị trí của UI text theo vị trí của người chơi trên màn hình.
    /// </summary>
    private void UpdateInteractionTextPosition()
    {
        if (interactionTextInstance != null && interactionTextInstance.gameObject.activeInHierarchy)
        {
            Vector3 worldPosition = transform.position + textOffset;
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            interactionTextInstance.transform.position = screenPosition;
        }
    }

    private void ShowInteractionPrompt(string text)
    {
        if (interactionTextInstance != null)
        {
            interactionTextInstance.text = $"[{interactionKey}] {text}";
            interactionTextInstance.gameObject.SetActive(true);
        }
    }

    private void ClearInteractionPrompt()
    {
        if (interactionTextInstance != null)
        {
            interactionTextInstance.gameObject.SetActive(false);
        }
    }
}