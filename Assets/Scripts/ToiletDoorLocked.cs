using System.Collections;
using UnityEngine;

public class ToiletDoorLocked : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 2.5f;
    public GameObject actionDisplay;
    public GameObject actionText;
    public GameObject actionTextClose;
    public GameObject extraCross;

    [Header("Door & Animation")]
    public GameObject doorObject;
    public string openAnimationName = "ToiletDoorOpen";
    public string closeAnimationName = "ToiletDoorClose";

    [Header("Player Control")]
    public MonoBehaviour playerMovementScript;

    [Header("Audio")]
    public AudioSource pryAudio;
    public AudioSource breathingAudio;
    public float pryDuration = 3f;

    private float distanceToPlayer;
    private bool isPryingOpen;
    private bool isOpen;
    private bool isUnlocked;
    private bool stuckLinePlayed;
    private bool crowbarHintLinePlayed;

    void Update()
    {
        distanceToPlayer = PlayerCasting.DistanceFromTarget;
    }

    void OnMouseOver()
    {
        if (isPryingOpen)
        {
            return;
        }

        bool inRange = distanceToPlayer <= interactionRange;

        if (inRange)
        {
            ShowPrompt();
        }
        else
        {
            HidePrompts();
        }

        if (inRange && Input.GetButtonDown("Action"))
        {
            HandleInteraction();
        }
    }

    void OnMouseExit()
    {
        HidePrompts();
    }

    void ShowPrompt()
    {
        if (actionDisplay != null) actionDisplay.SetActive(true);
        if (extraCross != null) extraCross.SetActive(true);

        if (isOpen)
        {
            if (actionTextClose != null) actionTextClose.SetActive(true);
            if (actionText != null) actionText.SetActive(false);
        }
        else
        {
            if (actionText != null) actionText.SetActive(true);
            if (actionTextClose != null) actionTextClose.SetActive(false);
        }

        if (HasCrowbar() && !isUnlocked && !crowbarHintLinePlayed)
        {
            MonologueManager.PlayMonologue("Cửa đã bị kẹt cứng, cái này sẽ có ích.", 3.5f, true, true);
            crowbarHintLinePlayed = true;
        }
    }

    void HidePrompts()
    {
        if (actionDisplay != null) actionDisplay.SetActive(false);
        if (actionText != null) actionText.SetActive(false);
        if (actionTextClose != null) actionTextClose.SetActive(false);
        if (extraCross != null) extraCross.SetActive(false);
    }

    void HandleInteraction()
    {
        if (!isUnlocked)
        {
            if (!HasCrowbar())
            {
                if (!stuckLinePlayed)
                {
                    MonologueManager.PlayMonologue("Kẹt cứng... Chết tiệt. Có thứ gì đó chặn nó lại từ bên trong. Mình không đủ sức.", 4f, true, true);
                    stuckLinePlayed = true;
                }
                return;
            }

            StartCoroutine(ForceDoorOpen());
            return;
        }

        if (isOpen)
        {
            StartCoroutine(CloseDoorRoutine());
        }
        else
        {
            StartCoroutine(OpenDoorRoutine());
        }
    }

    bool HasCrowbar()
    {
        return GlobalInventory.HasSpecificItem(ItemType.Crowbar);
    }

    IEnumerator ForceDoorOpen()
    {
        isPryingOpen = true;
        HidePrompts();
        ToggleUIObjects(false);

        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
        }

        if (pryAudio != null)
        {
            pryAudio.Play();
        }

        yield return new WaitForSeconds(pryDuration);

        PlayAnimation(openAnimationName);

        if (breathingAudio != null)
        {
            breathingAudio.Play();
        }

        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
        }

        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
        }

        ToggleUIObjects(true);

        isUnlocked = true;
        isOpen = true;
        isPryingOpen = false;
    }

    IEnumerator OpenDoorRoutine()
    {
        PlayAnimation(openAnimationName);
        isOpen = true;
        yield return null;
    }

    IEnumerator CloseDoorRoutine()
    {
        PlayAnimation(closeAnimationName);
        isOpen = false;
        yield return null;
    }

    void ToggleUIObjects(bool state)
    {
        if (actionDisplay != null) actionDisplay.SetActive(state);
        if (actionText != null) actionText.SetActive(state && !isOpen);
        if (actionTextClose != null) actionTextClose.SetActive(state && isOpen);
        if (extraCross != null) extraCross.SetActive(state);
    }


    void PlayAnimation(string animationName)
    {
        if (doorObject == null) return;
        var anim = doorObject.GetComponent<Animation>();
        if (anim == null) return;

        if (!string.IsNullOrEmpty(animationName) && anim.GetClip(animationName) != null)
        {
            anim.Play(animationName);
        }
        else
        {
            anim.Play();
        }
    }
}
