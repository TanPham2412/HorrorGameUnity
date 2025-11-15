using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ItemType
{
    Key,
    GuardKey,
    VHSTape,
    Flashlight
}

public class PickUpItem : MonoBehaviour
{
    [Header("Item Settings")]
    public ItemType itemType;
    
    [Header("Interaction Settings")]
    public float TheDistance;
    public GameObject ActionDisplay;
    public GameObject ActionText;
    public GameObject NameObject;
    public GameObject ExtraCross;
    
    [Header("Item Objects")]
    public GameObject FakeItem;
    public GameObject RealItem;
    public GameObject Player;
    
    void Update()
    {
        TheDistance = PlayerCasting.DistanceFromTarget;

        // Check for drop input based on item type (KHÔNG cho phép vứt flashlight)
        if (Input.GetKeyDown(KeyCode.Q) && GlobalInventory.HasSpecificItem(itemType) && itemType != ItemType.Flashlight)
        {
            DropItem();
        }
    }

    void OnMouseOver()
    {
        // Nếu là flashlight nhưng chưa được phép nhặt thì không hiện UI, không cho nhặt
        if (itemType == ItemType.Flashlight && !GlobalInventory.canPickupFlashlight)
        {
            return;
        }

        if (TheDistance <= 3)
        {
            ActionDisplay.SetActive(true);
            NameObject.SetActive(true);
            ActionText.SetActive(true);
            ExtraCross.SetActive(true);
        }
        if (Input.GetButtonDown("Action"))
        {
            if (TheDistance <= 3)
            {
                PickUpItemAction();
            }
        }
    }

    void OnMouseExit()
    {
        ExtraCross.SetActive(false);
        ActionDisplay.SetActive(false);
        ActionText.SetActive(false);
        NameObject.SetActive(false);
    }
    
    private bool HasItem()
    {
        switch (itemType)
        {
            case ItemType.Key:
                return GlobalInventory.hasKey;
            case ItemType.GuardKey:
                return GlobalInventory.hasGuardKey;
            case ItemType.VHSTape:
                return GlobalInventory.hasVHSTape;
            case ItemType.Flashlight:
                return GlobalInventory.hasFlashlight;
            default:
                return false;
        }
    }
    
    private void SetItemStatus(bool status)
    {
        // This method is now handled by GlobalInventory.SetCurrentItem() and GlobalInventory.ClearCurrentItem()
        // Kept for legacy compatibility if needed
        if (status)
        {
            GlobalInventory.SetCurrentItem(itemType, this);
        }
        else
        {
            GlobalInventory.ClearCurrentItem(itemType);
        }
    }
    
    private void PickUpItemAction()
    {
        // Handle two-slot inventory system
        if (itemType == ItemType.Flashlight)
        {
            // Flashlight has its own slot, no need to drop anything
        }
        else
        {
            // For regular items (Key, GuardKey, VHSTape), check if regular slot is occupied
            if (GlobalInventory.HasRegularItem())
            {
                // Drop the currently held regular item before picking up the new one
                DropCurrentlyHeldRegularItem();
            }
        }

        // Tách item ra khỏi parent (cái tủ)
        transform.SetParent(null);

        this.GetComponent<BoxCollider>().enabled = false;
        ExtraCross.SetActive(false);
        ActionDisplay.SetActive(false);
        ActionText.SetActive(false);
        NameObject.SetActive(false);
        FakeItem.SetActive(false);
        RealItem.SetActive(true);
        
        // Use new inventory system
        GlobalInventory.SetCurrentItem(itemType, this);
        
        // Debug log
        Debug.Log("Picked up: " + itemType + ", Regular item: " + GlobalInventory.currentRegularItem + ", Has flashlight: " + GlobalInventory.hasFlashlight);
    }
    
    private void DropItem()
    {
        this.GetComponent<BoxCollider>().enabled = true;
        ExtraCross.SetActive(false);
        ActionDisplay.SetActive(false);
        ActionText.SetActive(false);
        NameObject.SetActive(false);

        // Tách item ra khỏi parent để nó không bị đi theo tủ nữa
        transform.SetParent(null);
        FakeItem.transform.SetParent(null);

        Vector3 dropPos = Player.transform.position + Player.transform.forward * 0.5f;
        transform.position = dropPos;
        FakeItem.transform.position = dropPos;

        FakeItem.SetActive(true);
        RealItem.SetActive(false);
        
        // Use new inventory system
        GlobalInventory.ClearCurrentItem(itemType);
    }
    
    private void DropCurrentlyHeldRegularItem()
    {
        if (GlobalInventory.GetCurrentRegularItemScript() != null)
        {
            GlobalInventory.GetCurrentRegularItemScript().DropItem();
        }
    }
}
