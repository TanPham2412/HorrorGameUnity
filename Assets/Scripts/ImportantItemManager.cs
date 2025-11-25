using System.Collections.Generic;
using UnityEngine;

public class ImportantItemManager : MonoBehaviour
{
    public static ImportantItemManager Instance { get; private set; }

    [Header("Settings")]
    [Range(1, 5)] public int maxImportantItems = 5;
    public Transform importantItemHandSlot;
    public KeyCode cycleKey = KeyCode.F;

    [Header("Debug")]
    [SerializeField] private List<PickUpItem> importantItems = new();
    [SerializeField] private int currentImportantIndex = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
        if (item == null || importantItems.Contains(item)) return;

        if (importantItems.Count >= maxImportantItems)
        {
            Debug.LogWarning("ImportantItemManager: Reached max important items.");
            return;
        }

        importantItems.Add(item);
        PrepareItemForHand(item);

        if (importantItems.Count == 1 && !GlobalInventory.HasRegularItem())
        {
            EquipImportantItem(0);
        }
    }

    public void HideCurrentImportantItem()
    {
        if (!HasImportantEquipped()) return;

        var item = importantItems[currentImportantIndex];
        if (item != null)
        {
            item.SetHandItemActive(false);
        }
        currentImportantIndex = -1;
    }

    public bool HasImportantEquipped()
    {
        return currentImportantIndex >= 0 && currentImportantIndex < importantItems.Count;
    }

    private void CycleToNextImportantItem()
    {
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
            }
        }

        currentImportantIndex = index;
        var newItem = importantItems[currentImportantIndex];
        if (newItem != null)
        {
            PrepareItemForHand(newItem);
            newItem.SetHandItemActive(true);
        }
    }

    private void PrepareItemForHand(PickUpItem item)
    {
        if (item == null) return;

        if (importantItemHandSlot != null)
        {
            item.AttachImportantItemToHand(importantItemHandSlot);
        }

        item.SetHandItemActive(false);
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
}
