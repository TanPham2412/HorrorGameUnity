using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DoubleDoor : MonoBehaviour
{

    public float TheDistance;
    public GameObject ActionDisplay;
    public GameObject ActionText;
    public GameObject ActionText2;
    public GameObject LeftDoor;
    public GameObject RightDoor;
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
        LeftDoor.GetComponent<Animation>().Play("DoorLeftOpen");
        RightDoor.GetComponent<Animation>().Play("DoorRightOpen");
        DoorCreakSound.Play();
        doorIsOpen = true;
        yield return new WaitForSeconds(1f);
    }

    IEnumerator CloseTheDoor()
    {
        ExtraCross.SetActive(false);
        ActionDisplay.SetActive(false);
        ActionText2.SetActive(false);
        LeftDoor.GetComponent<Animation>().Play("DoorLeftClose");
        RightDoor.GetComponent<Animation>().Play("DoorRightClose");
        DoorCreakSound.Play();
        doorIsOpen = false;
        yield return new WaitForSeconds(1f);
    }
}