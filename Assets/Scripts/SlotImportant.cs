using UnityEngine;

public class SlotImportant : MonoBehaviour
{
    [Header("UI References")]
    public float TheDistance;
    public GameObject ActionDisplay;
    public GameObject ActionTextPlace;
    public GameObject ActionTextRetrieve;
    public GameObject NeedImportantItemText;
    public GameObject ExtraCross;

    [Header("Slot Settings")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private Transform placedItemAnchor;
    [SerializeField] private ManualItemVisual[] manualItemVisuals;

    [Header("State Tracking")]
    public PickUpItem placedImportantItem;
    public bool isCorrectItemPlaced;
    public ItemType requiredItemType = ItemType.RabbitDoll;

    private Collider slotCollider;
    private bool slotActive = true;
    private GameObject placedVisualInstance;
    private GameObject activeManualVisual;

    private void Awake()
    {
        slotCollider = GetComponent<Collider>();

        HideAllManualVisuals();

        if (placedImportantItem != null)
        {
            HandleItemPlaced(placedImportantItem);
        }
        else
        {
            RefreshSlotVisibility();
        }
    }

    private void Update()
    {
        TheDistance = PlayerCasting.DistanceFromTarget;
    }

    private void OnMouseOver()
    {
        if (!slotActive)
        {
            HideUI();
            return;
        }

        bool inRange = TheDistance <= interactionRange;
        if (!inRange)
        {
            HideUI();
            return;
        }

        bool hasPlacedItem = placedImportantItem != null;
        if (hasPlacedItem)
        {
            ActionDisplay?.SetActive(true);
            ActionTextPlace?.SetActive(false);
            if (ActionTextRetrieve != null)
            {
                ActionTextRetrieve.SetActive(true);
            }
            ExtraCross?.SetActive(true);

            if (Input.GetButtonDown("Action"))
            {
                RetrieveImportantItem();
            }
            return;
        }

        if (ImportantItemManager.Instance == null)
        {
            ShowNeedImportantItemPrompt();
            return;
        }

        if (!ImportantItemManager.Instance.TryGetCurrentImportantHand(out var currentItem) || currentItem == null)
        {
            ShowNeedImportantItemPrompt();
            return;
        }

        ActionDisplay?.SetActive(true);
        ActionTextRetrieve?.SetActive(false);
        if (ActionTextPlace != null) ActionTextPlace.SetActive(true);
        ExtraCross?.SetActive(true);

        if (Input.GetButtonDown("Action"))
        {
            PlaceImportantItem(currentItem);
        }
    }

    private void OnMouseExit()
    {
        HideUI();
    }

    private void PlaceImportantItem(PickUpItem item)
    {
        if (item == null || ImportantItemManager.Instance == null) return;

        if (!ImportantItemManager.Instance.RemoveImportantItem(item))
        {
            return;
        }

        placedImportantItem = item;
        isCorrectItemPlaced = item.itemType == requiredItemType;

        CreatePlacedVisual(item);

        RefreshSlotVisibility();
        HideUI();
    }

    private void CreatePlacedVisual(PickUpItem item)
    {
        DestroyPlacedVisual();

        GameObject source = item.FakeItem != null ? item.FakeItem : item.RealItem;
        if (source == null) return;

        if (placedItemAnchor == null)
        {
            placedItemAnchor = transform;
        }

        if (TryShowManualVisual(item.itemType))
        {
            return;
        }

        placedVisualInstance = Instantiate(source, placedItemAnchor.position, placedItemAnchor.rotation, placedItemAnchor);
        placedVisualInstance.transform.localPosition = Vector3.zero;
        placedVisualInstance.transform.localRotation = Quaternion.identity;
        placedVisualInstance.transform.localScale = Vector3.one;
        placedVisualInstance.name = source.name + "_SlotInstance";
        placedVisualInstance.SetActive(true);

        // Remove interaction components from the visual clone
        foreach (var collider in placedVisualInstance.GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }

        var pickupComponents = placedVisualInstance.GetComponentsInChildren<PickUpItem>();
        foreach (var comp in pickupComponents)
        {
            Destroy(comp);
        }
    }

    private void DestroyPlacedVisual()
    {
        HideAllManualVisuals();
        activeManualVisual = null;

        if (placedVisualInstance != null)
        {
            Destroy(placedVisualInstance);
            placedVisualInstance = null;
        }
    }

    private bool TryShowManualVisual(ItemType itemType)
    {
        if (manualItemVisuals == null)
        {
            return false;
        }

        foreach (var entry in manualItemVisuals)
        {
            if (entry == null || entry.visualObject == null)
            {
                continue;
            }

            bool match = entry.itemType == itemType;
            entry.visualObject.SetActive(match);

            if (match)
            {
                activeManualVisual = entry.visualObject;
                return true;
            }
        }

        activeManualVisual = null;
        return false;
    }

    private void HideAllManualVisuals()
    {
        if (manualItemVisuals == null)
        {
            return;
        }

        foreach (var entry in manualItemVisuals)
        {
            if (entry?.visualObject == null)
            {
                continue;
            }

            entry.visualObject.SetActive(false);
        }
    }

    private void HandleItemPlaced(PickUpItem item)
    {
        if (item == null) return;

        placedImportantItem = item;
        isCorrectItemPlaced = item.itemType == requiredItemType;

        if (placedItemAnchor != null && item.RealItem != null)
        {
            item.RealItem.transform.SetParent(placedItemAnchor, false);
            item.RealItem.transform.localPosition = Vector3.zero;
            item.RealItem.transform.localRotation = Quaternion.identity;
        }
        else if (item.RealItem != null)
        {
            item.RealItem.transform.position = transform.position;
        }

        if (item.RealItem != null)
        {
            item.RealItem.SetActive(true);
        }

        RefreshSlotVisibility();
    }

    private void RetrieveImportantItem()
    {
        if (placedImportantItem == null) return;

        placedImportantItem.SetHandItemActive(false);
        if (ImportantItemManager.Instance != null)
        {
            ImportantItemManager.Instance.RestoreImportantItemFromSlot(placedImportantItem);
        }

        DestroyPlacedVisual();

        placedImportantItem = null;
        isCorrectItemPlaced = false;

        RefreshSlotVisibility();
        HideUI();
    }

    private void RefreshSlotVisibility()
    {
        bool hasItem = placedImportantItem != null;
        slotActive = !hasItem || (hasItem && ImportantItemManager.Instance != null);

        if (slotCollider != null)
        {
            slotCollider.enabled = slotActive;
        }
    }

    private void HideUI()
    {
        ExtraCross?.SetActive(false);
        ActionDisplay?.SetActive(false);
        ActionTextPlace?.SetActive(false);
        ActionTextRetrieve?.SetActive(false);
        NeedImportantItemText?.SetActive(false);
    }

    private void ShowNeedImportantItemPrompt()
    {
        ActionDisplay?.SetActive(false);
        ActionTextPlace?.SetActive(false);
        ActionTextRetrieve?.SetActive(false);
        ExtraCross?.SetActive(false);
        if (NeedImportantItemText != null)
        {
            NeedImportantItemText.SetActive(true);
        }
    }
    [System.Serializable]
    private class ManualItemVisual
    {
        public ItemType itemType;
        public GameObject visualObject;
    }
}
