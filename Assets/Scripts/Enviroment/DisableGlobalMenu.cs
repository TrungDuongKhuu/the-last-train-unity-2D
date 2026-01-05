using UnityEngine;

public class DisableGlobalMenu : MonoBehaviour
{
    void Start()
    {
        if (MenuManager.instance != null)
        {
            MenuManager.instance.ShowHUD(false);
            MenuManager.instance.ShowPausePanel(false);
            MenuManager.instance.ShowSettingsPanel(false);
        }

        // Disable Inventory UI
        InventoryUI inv = Object.FindFirstObjectByType<InventoryUI>();
        if (inv != null)
            inv.gameObject.SetActive(false);

        Debug.Log("[DisableGlobalUI] Global Menu + InventoryUI disabled for puzzle scene.");
    }
}
