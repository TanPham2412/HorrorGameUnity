using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SecretDoorButton : MonoBehaviour
{
    [Header("Required UI References")]
    public float TheDistance;
    public GameObject ActionDisplay;
    public GameObject ActionText;
    public GameObject Door;
    public GameObject NameObject;
    public AudioSource SecretDoorOpen;
    private bool doorIsOpen = false;
    private bool isUnlocked = false;
    public GameObject ExtraCross;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private bool hideUntilUnlocked = true;
    [SerializeField] private string doorAnimationName = "SecretBasement";
    [SerializeField] private string completionFlag = "ImportantItemsCutscenePlayed";
    [SerializeField] private float disableDelay = 0.15f;
    [SerializeField] private GameObject[] visualsToToggle;
    [SerializeField] private bool allowRendererToggle = false;
    [Header("Activation Control")]
    [SerializeField] private bool disableObjectUntilCutsceneComplete = true;
    [SerializeField] private GameObject activationTargetOverride;

    private Collider interactionCollider;
    private Renderer[] cachedRenderers;
    private bool buttonUsed;
    private bool awaitingCutsceneCompletion;
    private GameObject activationTarget;

    private void Awake()
    {
        activationTarget = activationTargetOverride != null ? activationTargetOverride : gameObject;

        ImportantItemsCutscene.PostCutsceneMonologuesFinished += HandlePostCutsceneFinished;

        interactionCollider = GetComponent<Collider>();
        cachedRenderers = GetComponentsInChildren<Renderer>(true);

        if (!allowRendererToggle && cachedRenderers != null)
        {
            foreach (var renderer in cachedRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = false;
                }
            }
        }

        if (hideUntilUnlocked)
        {
            SetButtonVisible(false);
        }

        bool cutsceneAlreadyComplete = string.IsNullOrEmpty(completionFlag) || StoryFlagManager.HasFlag(completionFlag);

        if (!cutsceneAlreadyComplete && disableObjectUntilCutsceneComplete)
        {
            awaitingCutsceneCompletion = true;
            if (activationTarget != null && activationTarget.activeSelf)
            {
                activationTarget.SetActive(false);
            }
        }
        else if (cutsceneAlreadyComplete)
        {
            HandlePostCutsceneFinished();
        }
    }

    private void OnDisable()
    {
        HidePrompts();
    }

    private void OnDestroy()
    {
        ImportantItemsCutscene.PostCutsceneMonologuesFinished -= HandlePostCutsceneFinished;
    }

    private void HandleUnlockEvent()
    {
        if (isUnlocked) return;
        isUnlocked = true;
        doorIsOpen = false;
        buttonUsed = false;
        SetButtonVisible(true);
    }

    private void SetButtonVisible(bool visible)
    {
        if (!hideUntilUnlocked && isUnlocked)
        {
            visible = true;
        }

        if (activationTarget != null)
        {
            activationTarget.SetActive(visible);
        }

        if (interactionCollider != null)
        {
            interactionCollider.enabled = visible && activationTarget != null ? activationTarget.activeInHierarchy : visible;
        }

        if (visualsToToggle != null && visualsToToggle.Length > 0)
        {
            foreach (var target in visualsToToggle)
            {
                if (target != null) target.SetActive(visible);
            }
        }
        else if (allowRendererToggle && cachedRenderers != null)
        {
            foreach (var renderer in cachedRenderers)
            {
                if (renderer != null) renderer.enabled = visible;
            }
        }
    }

    private void Update()
    {
        TheDistance = PlayerCasting.DistanceFromTarget;
    }

    private void OnMouseOver()
    {
        if (interactionCollider != null && !interactionCollider.enabled) return;

        bool inRange = TheDistance <= interactionRange;

        if (!inRange)
        {
            HidePrompts();
            return;
        }

        if (!isUnlocked || buttonUsed)
        {
            HidePrompts();
            return;
        }

        if (NameObject != null) NameObject.SetActive(true);
        if (ExtraCross != null) ExtraCross.SetActive(true);
        if (ActionDisplay != null) ActionDisplay.SetActive(true);

        if (ActionText != null) ActionText.SetActive(!doorIsOpen);

        if (!doorIsOpen && !buttonUsed && Input.GetButtonDown("Action"))
        {
            StartCoroutine(ActivateSecretDoor());
        }
    }

    private void OnMouseExit()
    {
        HidePrompts();
    }

    private void HidePrompts()
    {
        if (NameObject != null) NameObject.SetActive(false);
        if (ExtraCross != null) ExtraCross.SetActive(false);
        if (ActionDisplay != null) ActionDisplay.SetActive(false);
        if (ActionText != null) ActionText.SetActive(false);
    }

    private IEnumerator ActivateSecretDoor()
    {
        buttonUsed = true;
        HidePrompts();

        if (SecretDoorOpen != null)
        {
            SecretDoorOpen.Play();
        }

        PlayDoorAnimation();

        doorIsOpen = true;
        yield return new WaitForSeconds(disableDelay);

        SetButtonVisible(false);
        gameObject.SetActive(false);
    }

    private void HandlePostCutsceneFinished()
    {
        if (awaitingCutsceneCompletion && activationTarget != null && !activationTarget.activeSelf)
        {
            activationTarget.SetActive(true);
        }

        awaitingCutsceneCompletion = false;
        HandleUnlockEvent();
    }

    private void PlayDoorAnimation()
    {
        if (Door == null) return;

        var legacyAnimation = Door.GetComponent<Animation>();
        if (legacyAnimation != null)
        {
            if (!string.IsNullOrWhiteSpace(doorAnimationName))
            {
                legacyAnimation.Play(doorAnimationName);
            }
            else
            {
                legacyAnimation.Play();
            }
            return;
        }

        var animator = Door.GetComponent<Animator>();
        if (animator != null)
        {
            if (!string.IsNullOrWhiteSpace(doorAnimationName))
            {
                animator.Play(doorAnimationName);
            }
            else
            {
                animator.SetTrigger("Open");
            }
        }
    }
}
