using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalInventory : MonoBehaviour
{
    // Legacy individual item flags (kept for compatibility)
    public static bool hasKey = false;
    public static bool hasGuardKey = false;
    public static bool hasVHSTape = false;
    public static bool hasFlashlight = false;
    public static bool hasSafeCard = false;
    public static bool hasCrowbar = false;

    // Two-slot inventory system: flashlight + one regular item
    public static ItemType? currentRegularItem = null;  // Key, GuardKey, VHSTape, SafeCard, Crowbar
    public static PickUpItem currentRegularItemScript = null;
    public static PickUpItem flashlightScript = null;   // Flashlight has its own slot

    // Chỉ cho nhặt flashlight sau khi xem xong băng cassette
    public static bool canPickupFlashlight = false;

    // Helper methods for two-slot inventory
    public static bool HasAnyItem()
    {
        return currentRegularItem != null || hasFlashlight;
    }

    public static bool HasSpecificItem(ItemType itemType)
    {
        if (itemType == ItemType.Flashlight)
        {
            return hasFlashlight;
        }
        return currentRegularItem == itemType;
    }

    public static bool HasRegularItem()
    {
        return currentRegularItem != null;
    }

    public static void SetCurrentItem(ItemType itemType, PickUpItem itemScript)
    {
        if (itemType == ItemType.Flashlight)
        {
            // Flashlight goes to special slot
            hasFlashlight = true;
            flashlightScript = itemScript;
        }
        else
        {
            // Other items go to regular slot (drop existing regular item if any)
            if (currentRegularItem != null)
            {
                SetLegacyFlag(currentRegularItem.Value, false);
            }
            
            currentRegularItem = itemType;
            currentRegularItemScript = itemScript;
            SetLegacyFlag(itemType, true);
        }
    }

    public static void ClearCurrentItem(ItemType itemType)
    {
        if (itemType == ItemType.Flashlight)
        {
            hasFlashlight = false;
            flashlightScript = null;
        }
        else if (currentRegularItem == itemType)
        {
            SetLegacyFlag(currentRegularItem.Value, false);
            currentRegularItem = null;
            currentRegularItemScript = null;
        }
    }

    public static PickUpItem GetCurrentRegularItemScript()
    {
        return currentRegularItemScript;
    }

    private static void ClearAllRegularItemFlags()
    {
        hasKey = false;
        hasGuardKey = false;
        hasVHSTape = false;
        hasSafeCard = false;
        hasCrowbar = false;
        // Don't clear flashlight here as it has its own slot
    }

    private static void SetLegacyFlag(ItemType itemType, bool value)
    {
        switch (itemType)
        {
            case ItemType.Key:
                hasKey = value;
                break;
            case ItemType.GuardKey:
                hasGuardKey = value;
                break;
            case ItemType.VHSTape:
                hasVHSTape = value;
                break;
            case ItemType.Flashlight:
                hasFlashlight = value;
                break;
            case ItemType.SafeCard:
                hasSafeCard = value;
                break;
            case ItemType.Crowbar:
                hasCrowbar = value;
                break;
        }
    }
}