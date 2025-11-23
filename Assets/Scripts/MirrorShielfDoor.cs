using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MirrorShelfDoor : MonoBehaviour
{

    public float TheDistance;
    public GameObject ActionDisplay;
    public GameObject ActionText;
    public GameObject ActionText2;
    public GameObject LeftDoor;
    public GameObject RightDoor;
    public GameObject NameObject;
    public AudioSource DoorCreakSound;
    public GameObject ExtraCross;
    private bool doorIsOpen = false;

    void Update()
    {
        TheDistance = PlayerCasting.DistanceFromTarget;
    }

    void OnMouseOver()
    {
        if (TheDistance <= 3)
        {
            NameObject.SetActive(true);
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
        else
        {
            NameObject.SetActive(false);
            ExtraCross.SetActive(false);
            ActionDisplay.SetActive(false);
            ActionText.SetActive(false);
            ActionText2.SetActive(false);
        }
        if (Input.GetButtonDown("Action"))
        {
            if (TheDistance <= 3)
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
        NameObject.SetActive(false);
        ExtraCross.SetActive(false);
        ActionDisplay.SetActive(false);
        ActionText.SetActive(false);
        ActionText2.SetActive(false);
    }

    IEnumerator OpenTheDoor()
    {
        ExtraCross.SetActive(false);
        ActionDisplay.SetActive(false);
        ActionText.SetActive(false);
        NameObject.SetActive(false);
        LeftDoor.GetComponent<Animation>().Play("MirrorDoorLOpen");
        RightDoor.GetComponent<Animation>().Play("MirrorDoorROpen");
        DoorCreakSound.Play();
        doorIsOpen = true;
        yield return new WaitForSeconds(1f);
    }

    IEnumerator CloseTheDoor()
    {
        ExtraCross.SetActive(false);
        ActionDisplay.SetActive(false);
        ActionText2.SetActive(false);
        NameObject.SetActive(false);
        LeftDoor.GetComponent<Animation>().Play("MirrorDoorLClose");
        RightDoor.GetComponent<Animation>().Play("MirrorDoorRClose");
        DoorCreakSound.Play();
        doorIsOpen = false;
        yield return new WaitForSeconds(1f);
    }
}