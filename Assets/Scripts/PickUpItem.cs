using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ItemType
{
    Key,
    GuardKey,
    VHSTape,
    VHSOfficeTape,
    Flashlight,
    SafeCard,
    Crowbar,
    RabbitDoll,
    Knife,
    BowOfPoison,
    MusicBox,
    Chains,
    OfficeKey,
    MachineRoomCard
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
    
    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip pickUpClip;
    public AudioClip dropClip;
    [Range(0f, 1f)] public float pickUpVolume = 1f;
    [Range(0f, 1f)] public float dropVolume = 1f;

    [Header("Pickup Special Effects")]
    public bool triggerSpecialEffects = false;
    public ItemType specialEffectItem = ItemType.SafeCard;
    public AudioSource screamAudio;
    public List<Light> blackoutLights = new();
    public GameObject playerFlashlightObject;
    public Light playerFlashlightLight;
    public MonoBehaviour flashlightToggleScript;
    public GameObject ghostJumpScare;
    public AudioSource breathingAudio;
    [TextArea(2, 4)] public string postGhostLine1;
    [TextArea(2, 4)] public string postGhostLine2;
    public float postGhostLineDuration = 4f;
    public float blackoutDuration = 2f;
    public float ghostVisibleDuration = 1f;
    public float secondBlackoutDuration = 0.5f;

    [Header("Pickup Monologue")]
    public bool playPickupMonologue = false;
    [TextArea(2, 4)] public string pickupMonologueLine;
    public float pickupMonologueDuration = 4f;
    public bool pickupLineAddsToLog = true;

    [Header("Important Item Settings")]
    public bool isImportantItem = false;
    [Tooltip("If true, capture the current RealItem local transform on Awake to reapply when equipping.")]
    public bool autoCaptureImportantHandOffsets = true;
    public Vector3 importantItemHandLocalPosition = Vector3.zero;
    public Vector3 importantItemHandLocalEuler = Vector3.zero;
    public Vector3 importantItemHandLocalScale = Vector3.one;
    
    private float actualDistanceToPlayer;
    private static bool crowbarMonologuePlayed = false;
    private bool specialEffectTriggered = false;
    private bool pickupMonologuePlayed = false;
    
    private void Awake()
    {
        if (GlobalInventory.IsImportantItemType(itemType))
        {
            isImportantItem = true;
        }

    }
    
    void Update()
    {
        TheDistance = PlayerCasting.DistanceFromTarget;
        
        // Tính khoảng cách thực tế từ player đến item
        if (Player != null)
        {
            actualDistanceToPlayer = Vector3.Distance(Player.transform.position, transform.position);
        }

        // Check for drop input based on item type (KHÔNG cho phép vứt flashlight)
        if (Input.GetKeyDown(KeyCode.Q) && !isImportantItem && GlobalInventory.HasSpecificItem(itemType) && itemType != ItemType.Flashlight)
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
            case ItemType.OfficeKey:
                return GlobalInventory.hasOfficeKey;
            case ItemType.VHSTape:
                return GlobalInventory.hasVHSTape;
            case ItemType.VHSOfficeTape:
                return GlobalInventory.hasVHSOfficeTape;
            case ItemType.Flashlight:
                return GlobalInventory.hasFlashlight;
            case ItemType.SafeCard:
                return GlobalInventory.hasSafeCard;
            case ItemType.MachineRoomCard:
                return GlobalInventory.hasMachineRoomCard;
            case ItemType.Crowbar:
                return GlobalInventory.hasCrowbar;
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
        if (isImportantItem)
        {
            HandleImportantItemPickup();
            return;
        }

        // Handle two-slot inventory system
        if (itemType == ItemType.Flashlight)
        {
            // Flashlight has its own slot, no need to drop anything
        }
        else
        {
            // Cất món quan trọng đang cầm đi trước khi chuyển sang món thường
            if (ImportantItemManager.Instance != null)
            {
                ImportantItemManager.Instance.HideCurrentImportantItem();
            }

            // For regular items, drop the item currently held (if any)
            if (GlobalInventory.HasRegularItem())
            {
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

        // Play pickup sound
        PlayPickupSound();

        if (itemType == ItemType.Crowbar && !crowbarMonologuePlayed)
        {
            MonologueManager.PlayMonologue("Cái này... nặng đấy. Ít nhất cũng hữu dụng hơn là tay không. Có thể nạy được thứ gì đó.", 4f, true, true);
            crowbarMonologuePlayed = true;
        }

        if (triggerSpecialEffects && itemType == specialEffectItem && !specialEffectTriggered)
        {
            specialEffectTriggered = true;
            StartCoroutine(HandlePickupEffects());
        }

        if (playPickupMonologue && !pickupMonologuePlayed && !string.IsNullOrWhiteSpace(pickupMonologueLine))
        {
            MonologueManager.PlayMonologue(pickupMonologueLine, pickupMonologueDuration, pickupLineAddsToLog, true);
            pickupMonologuePlayed = true;
        }
        
        // Debug log
        Debug.Log("Picked up: " + itemType + ", Regular item: " + GlobalInventory.currentRegularItem + ", Has flashlight: " + GlobalInventory.hasFlashlight);
    }

    private void HandleImportantItemPickup()
    {
        if (ImportantItemManager.Instance == null)
        {
            Debug.LogWarning("No ImportantItemManager present in scene. Cannot pick up important item.");
            return;
        }

        transform.SetParent(null);

        var collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = false;
        ExtraCross.SetActive(false);
        ActionDisplay.SetActive(false);
        ActionText.SetActive(false);
        NameObject.SetActive(false);
        FakeItem.SetActive(false);
        if (RealItem != null)
        {
            RealItem.SetActive(false);
        }

        GlobalInventory.SetImportantItemOwned(itemType, true);
        ImportantItemManager.Instance.TryAddImportantItem(this);

        if (triggerSpecialEffects && !specialEffectTriggered)
        {
            specialEffectTriggered = true;
            StartCoroutine(HandlePickupEffects());
        }

        if (playPickupMonologue && !pickupMonologuePlayed && !string.IsNullOrWhiteSpace(pickupMonologueLine))
        {
            MonologueManager.PlayMonologue(pickupMonologueLine, pickupMonologueDuration, pickupLineAddsToLog, true);
            pickupMonologuePlayed = true;
        }

        Debug.Log("Picked up important item: " + itemType);
    }

    private IEnumerator HandlePickupEffects()
    {
        bool flashlightLightWasOn = playerFlashlightLight != null && playerFlashlightLight.enabled;
        bool flashlightObjectWasActive = playerFlashlightObject != null && playerFlashlightObject.activeSelf;

        if (flashlightToggleScript != null)
        {
            flashlightToggleScript.enabled = false;
        }

        List<bool> previousStates = new();
        foreach (var light in blackoutLights)
        {
            if (light == null)
            {
                previousStates.Add(false);
                continue;
            }

            previousStates.Add(light.enabled);
            light.enabled = false;
        }

        SetPlayerFlashlightState(false);

        yield return new WaitForSeconds(blackoutDuration);

        for (int i = 0; i < blackoutLights.Count; i++)
        {
            Light light = blackoutLights[i];
            if (light == null) continue;
            bool wasEnabled = i < previousStates.Count ? previousStates[i] : true;
            light.enabled = wasEnabled;
        }

        SetPlayerFlashlightState(true);


        if (ghostJumpScare != null)
        {
            ghostJumpScare.SetActive(true);
        }

        if (screamAudio != null)
        {
            screamAudio.Play();
        }

        yield return new WaitForSeconds(ghostVisibleDuration);

        if (ghostJumpScare != null)
        {
            ghostJumpScare.SetActive(false);
        }

        foreach (var light in blackoutLights)
        {
            if (light == null) continue;
            light.enabled = false;
        }

        SetPlayerFlashlightState(false);

        yield return new WaitForSeconds(secondBlackoutDuration);

        for (int i = 0; i < blackoutLights.Count; i++)
        {
            Light light = blackoutLights[i];
            if (light == null) continue;
            bool wasEnabled = i < previousStates.Count ? previousStates[i] : true;
            light.enabled = wasEnabled;
        }

        if (breathingAudio != null)
        {
            breathingAudio.Play();
        }

        if (!string.IsNullOrWhiteSpace(postGhostLine1))
        {
            MonologueManager.PlayMonologue(postGhostLine1, postGhostLineDuration, true, true);
            yield return new WaitForSeconds(postGhostLineDuration);
        }

        if (!string.IsNullOrWhiteSpace(postGhostLine2))
        {
            MonologueManager.PlayMonologue(postGhostLine2, postGhostLineDuration, true, true);
            yield return new WaitForSeconds(postGhostLineDuration);
        }

        if (playerFlashlightLight != null)
        {
            playerFlashlightLight.enabled = flashlightLightWasOn;
        }

        if (playerFlashlightObject != null)
        {
            playerFlashlightObject.SetActive(flashlightObjectWasActive);
        }

        if (flashlightToggleScript != null)
        {
            flashlightToggleScript.enabled = true;
        }

        StoryFlagManager.SetFlag("SafeCardSequenceCompleted");
        AmbientMusicManager.Instance?.DisableRestroomMusic();
    }

    public void ForceDropFromInventory(bool playSound = false)
    {
        if (isImportantItem)
        {
            return;
        }
        DropItem(playSound);
    }

    public void SetHandItemActive(bool state)
    {
        if (RealItem != null)
        {
            RealItem.SetActive(state);
        }
    }

    public void AttachImportantItemToHand(Transform handSlot)
    {
        if (RealItem == null) return;

        Transform targetParent = handSlot != null ? handSlot : RealItem.transform.parent;

        if (targetParent != null)
        {
            if (autoCaptureImportantHandOffsets)
            {
                CaptureImportantItemOffsets(targetParent);
                autoCaptureImportantHandOffsets = false;
            }

            if (handSlot != null)
            {
                RealItem.transform.SetParent(handSlot, false);
            }

            RealItem.transform.localPosition = importantItemHandLocalPosition;
            RealItem.transform.localRotation = Quaternion.Euler(importantItemHandLocalEuler);
            RealItem.transform.localScale = importantItemHandLocalScale;
        }
    }

    private void CaptureImportantItemOffsets(Transform referenceParent)
    {
        if (RealItem == null || referenceParent == null) return;

        Transform realTransform = RealItem.transform;
        importantItemHandLocalPosition = referenceParent.InverseTransformPoint(realTransform.position);
        Quaternion relativeRotation = Quaternion.Inverse(referenceParent.rotation) * realTransform.rotation;
        importantItemHandLocalEuler = relativeRotation.eulerAngles;
        importantItemHandLocalScale = DivideVector(realTransform.lossyScale, referenceParent.lossyScale);
    }

    private static Vector3 DivideVector(Vector3 numerator, Vector3 denominator)
    {
        float SafeDiv(float a, float b)
        {
            return Mathf.Approximately(b, 0f) ? a : a / b;
        }

        return new Vector3(
            SafeDiv(numerator.x, denominator.x),
            SafeDiv(numerator.y, denominator.y),
            SafeDiv(numerator.z, denominator.z)
        );
    }

    private void SetPlayerFlashlightState(bool state)
    {
        if (playerFlashlightObject != null)
        {
            playerFlashlightObject.SetActive(state);
        }

        if (playerFlashlightLight != null)
        {
            playerFlashlightLight.enabled = state;
        }
    }
    
    private void DropItem(bool playDropSound = true)
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

        // Play drop sound only when requested (Q-drop)
        if (playDropSound)
        {
            PlayDropSound();
        }
    }
    
    private void DropCurrentlyHeldRegularItem()
    {
        if (GlobalInventory.GetCurrentRegularItemScript() != null)
        {
            // Auto-drop when picking another item -> don't play drop sound here
            GlobalInventory.GetCurrentRegularItemScript().DropItem(false);
        }
    }

    private void PlayPickupSound()
    {
        if (audioSource != null && pickUpClip != null)
        {
            audioSource.PlayOneShot(pickUpClip, pickUpVolume);
        }
    }

    private void PlayDropSound()
    {
        if (audioSource != null && dropClip != null)
        {
            audioSource.PlayOneShot(dropClip, dropVolume);
        }
    }
}
