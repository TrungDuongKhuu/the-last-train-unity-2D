using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using HeneGames.DialogueSystem; // Namespace cho hệ thống hội thoại

/// <summary>
/// Quản lý các đối tượng tương tác có điều kiện (cửa, tủ, đòn bẩy...).
/// Kích hoạt một sự kiện nếu điều kiện được đáp ứng, ngược lại sẽ hiển thị một đoạn độc thoại.
/// </summary>
public class ConditionalInteractable : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [Tooltip("Văn bản hiển thị khi người chơi ở gần. VD: 'Mở cửa'")]
    [SerializeField] private string interactionPrompt = "Tương tác";

    [Header("Condition")]
    [Tooltip("Điều kiện để tương tác thành công. Có thể được thay đổi bởi các script khác.")]
    public bool isConditionMet = false;

    [Header("Events")]
    [Tooltip("Sự kiện sẽ được kích hoạt khi tương tác thành công (isConditionMet = true).")]
    public UnityEvent onSuccess;

    [Header("Failure Monologue")]
    [Tooltip("Đoạn độc thoại của nhân vật khi tương tác thất bại (isConditionMet = false).")]
    [SerializeField] private List<NPC_Sentence> onFailureDialogue;

    [Header("UI Indicator")]
    [Tooltip("Prefab của dấu chấm than hoặc icon tương tác.")]
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private Vector3 indicatorOffset = new Vector3(0, 1.2f, 0);

    private GameObject indicatorInstance;
    private DialogueManager playerDialogueManager;

    void Start()
    {
        // Tìm DialogueManager của người chơi một lần duy nhất để tối ưu
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerDialogueManager = player.GetComponent<DialogueManager>();
        }
        else
        {
            Debug.LogError("ConditionalInteractable: Không tìm thấy đối tượng có tag 'Player'!", this);
        }
    }

    /// <summary>
    /// Hàm chính được gọi bởi PlayerInteractor khi người chơi nhấn nút tương tác.
    /// </summary>
    public void Interact()
    {
        if (isConditionMet)
        {
            // Điều kiện được đáp ứng -> Kích hoạt sự kiện thành công
            Debug.Log("Tương tác thành công!", this);
            onSuccess.Invoke();
        }
        else
        {
            // Điều kiện không được đáp ứng -> Bắt đầu đoạn độc thoại thất bại
            Debug.Log("Tương tác thất bại, bắt đầu độc thoại.", this);
            if (playerDialogueManager != null && onFailureDialogue.Count > 0)
            {
                playerDialogueManager.StartCustomDialogue(onFailureDialogue);
            }
            else
            {
                Debug.LogWarning("Không có DialogueManager hoặc không có câu thoại thất bại nào được gán.", this);
            }
        }
    }

    #region Interface Implementation
    public string GetInteractionText()
    {
        return interactionPrompt;
    }

    public void ShowIndicator()
    {
        if (indicatorPrefab != null && indicatorInstance == null)
        {
            Canvas mainCanvas = FindFirstObjectByType<Canvas>();
            if (mainCanvas != null)
            {
                indicatorInstance = Instantiate(indicatorPrefab, mainCanvas.transform);
            }
            else
            {
                Debug.LogError("Không tìm thấy Canvas trong Scene để hiển thị Indicator.", this);
                return;
            }
        }

        if (indicatorInstance != null)
        {
            indicatorInstance.SetActive(true);
            UpdateIndicatorPosition();
        }
    }

    public void HideIndicator()
    {
        if (indicatorInstance != null)
        {
            indicatorInstance.SetActive(false);
        }
    }
    #endregion

    #region Helper Methods
    private void UpdateIndicatorPosition()
    {
        if (indicatorInstance != null && indicatorInstance.activeSelf)
        {
            if(Camera.main != null)
            {
                indicatorInstance.transform.position = Camera.main.WorldToScreenPoint(transform.position + indicatorOffset);
            }
        }
    }

    private void Update()
    {
        // Luôn cập nhật vị trí của indicator nếu nó đang hiển thị
        UpdateIndicatorPosition();
    }

    private void OnDestroy()
    {
        // Phá hủy indicator khi đối tượng bị phá hủy để tránh rác trong Hierarchy
        if (indicatorInstance != null)
        {
            Destroy(indicatorInstance);
        }
    }
    #endregion
}
