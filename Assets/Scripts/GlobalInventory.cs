    using System.Collections.Generic;
using UnityEngine;

public class GlobalInventory : MonoBehaviour
{
    // Legacy individual item flags (kept for compatibility)
    public static bool hasKey = false;
    public static bool hasGuardKey = false;
    public static bool hasVHSTape = false;
    public static bool hasVHSOfficeTape = false;
    public static bool hasFlashlight = false;
    public static bool hasSafeCard = false;
    public static bool hasCrowbar = false;

    private static readonly HashSet<ItemType> importantItemsOwned = new();

    // Two-slot inventory system: flashlight + one regular item
    public static ItemType? currentRegularItem = null;  // Key, GuardKey, VHSTape, VHSOfficeTape, SafeCard, Crowbar
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

        if (IsImportantItemType(itemType))
        {
            return importantItemsOwned.Contains(itemType);
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
            hasFlashlight = true;
            flashlightScript = itemScript;
            return;
        }

        if (IsImportantItemType(itemType))
        {
            SetImportantItemOwned(itemType, true);
            return;
        }

        if (currentRegularItem != null)
        {
            SetLegacyFlag(currentRegularItem.Value, false);
        }

        currentRegularItem = itemType;
        currentRegularItemScript = itemScript;
        SetLegacyFlag(itemType, true);
    }

    public static void ClearCurrentItem(ItemType itemType)
    {
        if (itemType == ItemType.Flashlight)
        {
            hasFlashlight = false;
            flashlightScript = null;
            return;
        }

        if (IsImportantItemType(itemType))
        {
            SetImportantItemOwned(itemType, false);
            return;
        }

        if (currentRegularItem == itemType)
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

    public static void ForceDropCurrentRegularItem()
    {
        if (currentRegularItemScript != null)
        {
            currentRegularItemScript.ForceDropFromInventory(true);
        }
    }

    public static void SetImportantItemOwned(ItemType itemType, bool owned)
    {
        if (!IsImportantItemType(itemType)) return;

        if (owned)
        {
            importantItemsOwned.Add(itemType);
        }
        else
        {
            importantItemsOwned.Remove(itemType);
        }
    }

    public static bool IsImportantItemType(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.RabbitDoll:
            case ItemType.Knife:
            case ItemType.BowOfPoison:
            case ItemType.MusicBox:
            case ItemType.Chains:
                return true;
            default:
                return false;
        }
    }

    public static IReadOnlyCollection<ItemType> GetImportantItems()
    {
        return importantItemsOwned;
    }

    private static void ClearAllRegularItemFlags()
    {
        hasKey = false;
        hasGuardKey = false;
        hasVHSTape = false;
        hasVHSOfficeTape = false;
        hasSafeCard = false;
        hasCrowbar = false;
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
            case ItemType.VHSOfficeTape:
                hasVHSOfficeTape = value;
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