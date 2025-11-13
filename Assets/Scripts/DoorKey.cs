using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DoorKey : MonoBehaviour
{

    public float TheDistance;
    public GameObject ActionDisplay;
    public GameObject ActionText;
    public GameObject ActionText2;
    public GameObject LockedText;
    public GameObject Door;
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
            if (isUnlocked == false && GlobalInventory.hasKey == false)
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
            if (TheDistance <= 3 && (GlobalInventory.hasKey == true || isUnlocked == true))
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
}
