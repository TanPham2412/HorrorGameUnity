using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorType
{
    GenericDoor,
    GuardRoom,
    OfficeDoor
}

public class DoorKey : MonoBehaviour
{

    [Header("Door Settings")]
    public DoorType doorType = DoorType.GenericDoor;

    [Header("Interaction Settings")]
    public float TheDistance;
    public GameObject ActionDisplay;
    public GameObject ActionText;
    public GameObject ActionText2;
    public GameObject LockedText;
    [TextArea(2, 4)] public string lockedMonologue = "Cánh cửa đã bị khóa rồi, mình phải đi tìm chìa khóa, có lẽ nó ở trong phòng bảo vệ.";
    public float lockedMonologueDuration = 4f;
    public bool lockedMonologueAddsToLog = false;
    public GameObject Door;
    public GameObject NameObject;
    public AudioSource DoorCreakSound;
    private bool doorIsOpen = false;
    private bool isUnlocked = false;
    public GameObject ExtraCross;

    public bool IsUnlocked => isUnlocked;
    void Update()
    {
        TheDistance = PlayerCasting.DistanceFromTarget;
    }

    void OnMouseOver()
    {
        if (TheDistance <= 3)
        {
            NameObject.SetActive(true);
            if (isUnlocked == false && !HasRequiredKey())
            {
                LockedText.SetActive(true);
                if (doorType == DoorType.OfficeDoor)
                {
                    MaybePlayLockedMonologue();
                }
            }
            else
            {
                ActionDisplay.SetActive(true);
                ExtraCross.SetActive(true);
                if (doorIsOpen == false)
                {
                    ActionText.SetActive(true);
                }
                else
                {
                    ActionText2.SetActive(true);
                }
            }
        }
        else
        {
            HideAllDoorUI();
        }
        if (Input.GetButtonDown("Action"))
        {
            if (TheDistance <= 3 && (HasRequiredKey() || isUnlocked == true))
            {
                if (doorIsOpen == false)
                {
                    StartCoroutine(OpenTheDoor());
                }
                else
                {
                    StartCoroutine(CloseTheDoor());
                }
            }
        }
    }

    void OnMouseExit()
    {
        HideAllDoorUI();
    }

    IEnumerator OpenTheDoor()
    {
        ExtraCross.SetActive(false);
        ActionDisplay.SetActive(false);
        ActionText.SetActive(false);
        Door.GetComponent<Animation>().Play("DoorOpenAnimation");
        DoorCreakSound.Play();
        doorIsOpen = true;
        isUnlocked = true;
        yield return new WaitForSeconds(1f);
    }

    IEnumerator CloseTheDoor()
    {
        ExtraCross.SetActive(false);
        ActionDisplay.SetActive(false);
        ActionText2.SetActive(false);
        Door.GetComponent<Animation>().Play("DoorCloseAnimation");
        DoorCreakSound.Play();
        doorIsOpen = false;
        yield return new WaitForSeconds(1f);
    }

    private bool HasRequiredKey()
    {
        switch (doorType)
        {
            case DoorType.GenericDoor:
                return GlobalInventory.hasKey;
            case DoorType.GuardRoom:
                return GlobalInventory.hasGuardKey;
            case DoorType.OfficeDoor:
                return GlobalInventory.hasOfficeKey;
            default:
                return false;
        }
    }

    public void UnlockDoorExternally(bool openImmediately = false)
    {
        if (isUnlocked && !openImmediately)
        {
            return;
        }

        isUnlocked = true;
        if (LockedText != null) LockedText.SetActive(false);

        if (openImmediately && !doorIsOpen)
        {
            StartCoroutine(OpenTheDoor());
        }
    }

    public void ForceCloseAndDisableInteraction()
    {
        StartCoroutine(ForceCloseAndDisableRoutine());
    }

    private IEnumerator ForceCloseAndDisableRoutine()
    {
        if (doorIsOpen)
        {
            yield return StartCoroutine(CloseTheDoor());
        }

        HideAllDoorUI();
        enabled = false;
    }

    private void MaybePlayLockedMonologue()
    {
        if (string.IsNullOrWhiteSpace(lockedMonologue)) return;
        MonologueManager.PlayMonologue(lockedMonologue, lockedMonologueDuration > 0 ? lockedMonologueDuration : 4f, lockedMonologueAddsToLog, true);
    }

    private void HideAllDoorUI()
    {
        if (ExtraCross != null) ExtraCross.SetActive(false);
        if (ActionDisplay != null) ActionDisplay.SetActive(false);
        if (ActionText != null) ActionText.SetActive(false);
        if (ActionText2 != null) ActionText2.SetActive(false);
        if (LockedText != null) LockedText.SetActive(false);
        if (NameObject != null) NameObject.SetActive(false);
    }
}
