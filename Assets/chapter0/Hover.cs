using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Hover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Color32 normalColor = new Color32(115, 0, 0, 255);  // màu gốc
    private Color32 hoverColor  = new Color32(204, 0, 0, 255);  // màu hover

    private TextMeshProUGUI textMeshPro;

    void Start()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
        if (textMeshPro == null)
        {
            Debug.LogError("TextMeshProUGUI component not found on this GameObject.");
        }
        else
        {
            textMeshPro.color = normalColor; // thiết lập màu ban đầu
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        textMeshPro.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        textMeshPro.color = normalColor;
    }
}
