using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorType
{
    GenericDoor,
    GuardRoom
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
    public GameObject Door;
    public GameObject NameObject;
    public AudioSource DoorCreakSound;
    private bool doorIsOpen = false;
    private bool isUnlocked = false;
    public GameObject ExtraCross;
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
            ExtraCross.SetActive(false);
            ActionDisplay.SetActive(false);
            ActionText.SetActive(false);
            ActionText2.SetActive(false);
            LockedText.SetActive(false);
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
        ExtraCross.SetActive(false);
        ActionDisplay.SetActive(false);
        ActionText.SetActive(false);
        ActionText2.SetActive(false);
        LockedText.SetActive(false);
        NameObject.SetActive(false);
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
            default:
                return false;
        }
    }
}
