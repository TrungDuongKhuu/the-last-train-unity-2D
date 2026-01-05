using UnityEngine;

// Interface định nghĩa một đối tượng có thể tương tác
public interface IInteractable
{
    void Interact();
    string GetInteractionText();

    // Thêm 2 hàm này
    void ShowIndicator();
    void HideIndicator();
}