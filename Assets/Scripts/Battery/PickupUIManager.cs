using UnityEngine;
using TMPro;

public class PickupUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pickupPromptPanel;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Prompt Messages")]
    [SerializeField] private string pickupBatteryText = "Press E to pick up";
    [SerializeField] private string dropBatteryText = "Press E to drop battery";
    [SerializeField] private string insertBatteryText = "Press E to insert battery";
    [SerializeField] private string weaponBlockedText = "Press E to drop battery before using weapon";

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private bool _isVisible = false;

    void Start()
    {
        HidePrompt();

        if (enableDebugLogs)
        {
            Debug.Log("[PickupUIManager] UI initialized");
        }
    }

    public void ShowPickupPrompt()
    {
        ShowPrompt(pickupBatteryText);

        if (enableDebugLogs)
        {
            Debug.Log("[PickupUIManager] Showing pickup prompt");
        }
    }

    public void ShowDropPrompt()
    {
        ShowPrompt(dropBatteryText);

        if (enableDebugLogs)
        {
            Debug.Log("[PickupUIManager] Showing drop prompt");
        }
    }

    public void ShowInsertPrompt()
    {
        ShowPrompt(insertBatteryText);

        if (enableDebugLogs)
        {
            Debug.Log("[PickupUIManager] Showing insert prompt");
        }
    }

    public void ShowWeaponBlockedPrompt()
    {
        ShowPrompt(weaponBlockedText);

        if (enableDebugLogs)
        {
            Debug.Log("[PickupUIManager] Showing weapon blocked prompt");
        }
    }

    private void ShowPrompt(string message)
    {
        if (pickupPromptPanel != null)
        {
            pickupPromptPanel.SetActive(true);
        }

        if (promptText != null)
        {
            promptText.text = message;
        }

        _isVisible = true;
    }

    public void HidePrompt()
    {
        if (pickupPromptPanel != null)
        {
            pickupPromptPanel.SetActive(false);
        }

        _isVisible = false;

        if (enableDebugLogs)
        {
            Debug.Log("[PickupUIManager] Hiding prompt");
        }
    }

    public bool IsVisible => _isVisible;

    [ContextMenu("Test Show Pickup Prompt")]
    public void TestShowPickup()
    {
        ShowPickupPrompt();
    }

    [ContextMenu("Test Show Insert Prompt")]
    public void TestShowInsert()
    {
        ShowInsertPrompt();
    }

    [ContextMenu("Test Hide Prompt")]
    public void TestHidePrompt()
    {
        HidePrompt();
    }
}