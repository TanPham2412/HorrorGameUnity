using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ToiletDoor : MonoBehaviour
{

    public float TheDistance;
    public GameObject ActionDisplay;
    public GameObject ActionText;
    public GameObject ActionText2;
    public GameObject Door;
    public AudioSource DoorCreakSound;
    private bool doorIsOpen = false;
    public GameObject ExtraCross;
    void Update()
    {
        TheDistance = PlayerCasting.DistanceFromTarget;
    }

    void OnMouseOver()
    {
        if (TheDistance <= 3)
        {
            ActionDisplay.SetActive(true);
            ExtraCross.SetActive (true);
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
        Door.GetComponent<Animation>().Play("ToiletDoorOpen");
        DoorCreakSound.Play();
        doorIsOpen = true;
        yield return new WaitForSeconds(1f);
    }

    IEnumerator CloseTheDoor()
    {
        ExtraCross.SetActive(false);
        ActionDisplay.SetActive(false);
        ActionText2.SetActive(false);
        Door.GetComponent<Animation>().Play("ToiletDoorClose");
        DoorCreakSound.Play();
        doorIsOpen = false;
        yield return new WaitForSeconds(1f);
    }
}