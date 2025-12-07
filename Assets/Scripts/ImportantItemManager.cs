using System;
using System.Collections.Generic;
using UnityEngine;

public class ImportantItemManager : MonoBehaviour
{
    public static ImportantItemManager Instance { get; private set; }
    public static event Action OnAllImportantItemsCollected;

    [Header("Settings")]
    [Range(1, 5)] public int maxImportantItems = 5;
    public Transform importantItemHandSlot;
    public KeyCode cycleKey = KeyCode.F;
    [Tooltip("Optional pre-placed visuals that are enabled when their important item is equipped.")]
    public List<ImportantItemHandVisual> presetHandVisuals = new();

    [Header("Debug")]
    [SerializeField] private List<PickUpItem> importantItems = new();
    [SerializeField] private int currentImportantIndex = -1;

    private readonly Dictionary<ItemType, GameObject> visualLookup = new();

    [Serializable]
    public class ImportantItemHandVisual
    {
        public ItemType itemType;
        public GameObject visualObject;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        maxImportantItems = Mathf.Clamp(maxImportantItems, 1, 5);
        importantItems.Clear();
        currentImportantIndex = -1;

        visualLookup.Clear();
        foreach (var visual in presetHandVisuals)
        {
            if (visual == null || visual.visualObject == null) continue;
            if (visualLookup.ContainsKey(visual.itemType)) continue;
            visual.visualObject.SetActive(false);
            visualLookup.Add(visual.itemType, visual.visualObject);
        }
    }

    private void Update()
    {
        if (importantItems.Count == 0) return;

        if (Input.GetKeyDown(cycleKey))
        {
            CycleToNextImportantItem();
        }
    }

    public void TryAddImportantItem(PickUpItem item)
    {
        if (item == null) return;

        CleanupInvalidEntries();

        if (importantItems.Exists(existing => existing != null && existing.itemType == item.itemType))
        {
            return;
        }

        if (importantItems.Count >= maxImportantItems)
        {
            Debug.LogWarning("ImportantItemManager: Reached max important items.");
            return;
        }

        importantItems.Add(item);
        PrepareItemForHand(item);

        bool shouldAutoEquip = !GlobalInventory.HasRegularItem() && !HasImportantEquipped();
        if (shouldAutoEquip)
        {
            EquipImportantItem(importantItems.Count - 1);
        }

        if (importantItems.Count >= maxImportantItems)
        {
            OnAllImportantItemsCollected?.Invoke();
        }
    }

    public void HideCurrentImportantItem()
    {
        if (!HasImportantEquipped()) return;

        var item = importantItems[currentImportantIndex];
        if (item != null)
        {
            item.SetHandItemActive(false);
            TogglePresetVisual(item.itemType, false);
        }
        currentImportantIndex = -1;
    }

    public bool HasImportantEquipped()
    {
        return currentImportantIndex >= 0 && currentImportantIndex < importantItems.Count;
    }

    private void CycleToNextImportantItem()
    {
        CleanupInvalidEntries();

        if (importantItems.Count == 0) return;

        // Nếu đang cầm item thường -> bắt buộc drop trước
        if (GlobalInventory.HasRegularItem())
        {
            GlobalInventory.ForceDropCurrentRegularItem();
        }

        int startIndex = currentImportantIndex;
        int attempts = importantItems.Count;

        for (int i = 0; i < attempts; i++)
        {
            startIndex = (startIndex + 1) % importantItems.Count;

            if (importantItems[startIndex] != null)
            {
                EquipImportantItem(startIndex);
                return;
            }
        }
    }

    private void EquipImportantItem(int index)
    {
        if (index < 0 || index >= importantItems.Count) return;

        if (currentImportantIndex == index)
        {
            return;
        }

        if (HasImportantEquipped())
        {
            var currentItem = importantItems[currentImportantIndex];
            if (currentItem != null)
            {
                currentItem.SetHandItemActive(false);
                TogglePresetVisual(currentItem.itemType, false);
            }
        }

        currentImportantIndex = index;
        var newItem = importantItems[currentImportantIndex];
        if (newItem != null)
        {
            PrepareItemForHand(newItem);
            if (!TogglePresetVisual(newItem.itemType, true))
            {
                newItem.SetHandItemActive(true);
            }
        }
    }

    private void PrepareItemForHand(PickUpItem item)
    {
        if (item == null) return;

        if (visualLookup.ContainsKey(item.itemType))
        {
            item.SetHandItemActive(false);
            return;
        }

        if (item.RealItem == null)
        {
            Debug.LogWarning($"ImportantItemManager: {item.name} is missing its RealItem reference.");
            return;
        }

        if (importantItemHandSlot != null)
        {
            item.AttachImportantItemToHand(importantItemHandSlot);
        }

        item.SetHandItemActive(false);
    }

    private void CleanupInvalidEntries()
    {
        bool removedAny = false;
        for (int i = importantItems.Count - 1; i >= 0; i--)
        {
            if (importantItems[i] == null)
            {
                importantItems.RemoveAt(i);
                removedAny = true;
            }
        }

        if (!removedAny)
        {
            return;
        }

        if (importantItems.Count == 0)
        {
            currentImportantIndex = -1;
            return;
        }

        currentImportantIndex = Mathf.Clamp(currentImportantIndex, -1, importantItems.Count - 1);
    }

    public bool TryGetCurrentImportantHand(out PickUpItem item)
    {
        if (HasImportantEquipped())
        {
            item = importantItems[currentImportantIndex];
            return item != null;
        }

        item = null;
        return false;
    }

    public bool RemoveImportantItem(PickUpItem item)
    {
        if (item == null) return false;

        int index = importantItems.IndexOf(item);
        if (index < 0)
        {
            return false;
        }

        if (currentImportantIndex == index)
        {
            if (importantItems[currentImportantIndex] != null)
            {
                importantItems[currentImportantIndex].SetHandItemActive(false);
                TogglePresetVisual(importantItems[currentImportantIndex].itemType, false);
            }
            currentImportantIndex = -1;
        }

        importantItems.RemoveAt(index);

        if (currentImportantIndex > index)
        {
            currentImportantIndex--;
        }

        if (importantItems.Count == 0)
        {
            currentImportantIndex = -1;
        }
        else
        {
            currentImportantIndex = Mathf.Clamp(currentImportantIndex, -1, importantItems.Count - 1);
        }

        return true;
    }

    public void RestoreImportantItemFromSlot(PickUpItem item)
    {
        if (item == null) return;
        CleanupInvalidEntries();

        if (importantItems.Contains(item))
        {
            return;
        }

        importantItems.Add(item);
        PrepareItemForHand(item);

        bool shouldEquip = !HasImportantEquipped();
        if (shouldEquip)
        {
            EquipImportantItem(importantItems.Count - 1);
        }
    }

    private bool TogglePresetVisual(ItemType itemType, bool visible)
    {
        if (!visualLookup.TryGetValue(itemType, out var visual) || visual == null)
        {
            return false;
        }

        visual.SetActive(visible);
        return true;
    }
}
