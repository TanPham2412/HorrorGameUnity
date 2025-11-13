using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ItemType
{
    Key,
    VHSTape
}

public class PickUpItem : MonoBehaviour
{
    [Header("Item Settings")]
    public ItemType itemType;
    
    [Header("Interaction Settings")]
    public float TheDistance;
    public GameObject ActionDisplay;
    public GameObject ActionText;
    public GameObject ExtraCross;
    
    [Header("Item Objects")]
    public GameObject FakeItem;
    public GameObject RealItem;
    public GameObject Player;
    
    void Update()
    {
        TheDistance = PlayerCasting.DistanceFromTarget;

        // Check for drop input based on item type
        if (Input.GetKeyDown(KeyCode.Q) && HasItem())
        {
            DropItem();
        }
    }

    void OnMouseOver()
    {
        if (TheDistance <= 3)
        {
            ActionDisplay.SetActive(true);
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
    }
    
    private bool HasItem()
    {
        switch (itemType)
        {
            case ItemType.Key:
                return GlobalInventory.hasKey;
            case ItemType.VHSTape:
                return GlobalInventory.hasVHSTape;
            default:
                return false;
        }
    }
    
    private void SetItemStatus(bool status)
    {
        switch (itemType)
        {
            case ItemType.Key:
                GlobalInventory.hasKey = status;
                break;
            case ItemType.VHSTape:
                GlobalInventory.hasVHSTape = status;
                break;
        }
    }
    
    private void PickUpItemAction()
    {
        // Tách item ra khỏi parent (cái tủ)
        transform.SetParent(null);

        this.GetComponent<BoxCollider>().enabled = false;
        ExtraCross.SetActive(false);
        ActionDisplay.SetActive(false);
        ActionText.SetActive(false);
        FakeItem.SetActive(false);
        RealItem.SetActive(true);
        SetItemStatus(true);
        
        // Debug log
        Debug.Log("Picked up: " + itemType + ", hasVHSTape = " + GlobalInventory.hasVHSTape);
    }
    
    private void DropItem()
    {
        this.GetComponent<BoxCollider>().enabled = true;
        ExtraCross.SetActive(false);
        ActionDisplay.SetActive(false);
        ActionText.SetActive(false);

        // Tách item ra khỏi parent để nó không bị đi theo tủ nữa
        transform.SetParent(null);
        FakeItem.transform.SetParent(null);

        Vector3 dropPos = Player.transform.position + Player.transform.forward * 0.5f;
        transform.position = dropPos;
        FakeItem.transform.position = dropPos;

        FakeItem.SetActive(true);
        RealItem.SetActive(false);
        SetItemStatus(false);
    }
}
