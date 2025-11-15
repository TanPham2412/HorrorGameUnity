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
    
    [Header("Pickup Settings")]
    public float pickupDistance = 3f; // Khoảng cách tối đa để nhặt item
    
    private float actualDistanceToPlayer;
    
    void Update()
    {
        TheDistance = PlayerCasting.DistanceFromTarget;
        
        // Tính khoảng cách thực tế từ player đến item
        if (Player != null)
        {
            actualDistanceToPlayer = Vector3.Distance(Player.transform.position, transform.position);
        }

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

        // Sử dụng khoảng cách thực tế thay vì chỉ dựa vào raycast
        bool isInRange = actualDistanceToPlayer <= pickupDistance;

        if (isInRange)
        {
            ActionDisplay.SetActive(true);
            NameObject.SetActive(true);
            ActionText.SetActive(true);
            ExtraCross.SetActive(true);
        }
        if (Input.GetButtonDown("Action"))
        {
            if (isInRange)
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

        // Thả vật phẩm xuống bề mặt ngay bên dưới vị trí tay cầm (RealItem)
        Vector3 dropPos;
        RaycastHit hit;
        float surfaceOffset = 0.02f; // Đặt cách bề mặt một khoảng rất nhỏ

        // Chọn điểm bắt đầu raycast: ưu tiên vị trí RealItem (vật đang cầm trên tay)
        Vector3 origin;
        if (RealItem != null)
        {
            origin = RealItem.transform.position + Vector3.up * 0.2f;
        }
        else if (Player != null)
        {
            origin = Player.transform.position + Vector3.up * 1.0f;
        }
        else
        {
            origin = transform.position + Vector3.up * 1.0f;
        }

        // 1. Raycast từ trên xuống để tìm bề mặt (bàn, tủ, sàn...)
        if (Physics.Raycast(origin, Vector3.down, out hit, 5f))
        {
            dropPos = hit.point + Vector3.up * surfaceOffset;
        }
        else
        {
            // 2. Nếu không có bề mặt bên dưới tay, thả xuống sàn phía trước player
            Vector3 forwardPos;
            if (Player != null)
            {
                forwardPos = Player.transform.position + Player.transform.forward * 0.5f;
            }
            else
            {
                forwardPos = transform.position + transform.forward * 0.5f;
            }

            if (Physics.Raycast(forwardPos + Vector3.up * 2f, Vector3.down, out hit, 5f))
            {
                dropPos = hit.point + Vector3.up * surfaceOffset;
            }
            else
            {
                // 3. Fallback: đặt ở vị trí mặc định phía trước player
                dropPos = forwardPos;
            }
        }

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
