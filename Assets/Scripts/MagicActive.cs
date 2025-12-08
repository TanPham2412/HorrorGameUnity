using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MagicActive : MonoBehaviour
{
    public static event Action SuccessVideoStarted;

    [Header("UI References")]
    public float TheDistance;
    public GameObject ActionDisplay;
    public GameObject ActionTextActivate;
    public GameObject NeedAllItemsText;
    public GameObject ExtraCross;

    [Header("Trigger Settings")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private SlotImportant[] slots;
    [SerializeField] private Animation[] smallRingAnimations;
    [SerializeField] private Animation bigRingAnimation;
    [SerializeField] private float bigRingDelay = 3f;
    [SerializeField] private string successMonologue = "Thành...Thành công rồi!!!";
    [SerializeField] private float successMonologueDuration = 3f;
    [SerializeField] private float successVideoDelayAfterMonologue = 3f;
    [SerializeField] private VideoPlaybackConfig successVideoConfig;
    [SerializeField] private EndCreditConfig successCredits;

    [Header("Failure Handling")]
    [SerializeField] private string failureMonologue = "THẤT BẠI RỒI!!! Có lẽ đã đặt SAI vị trí.";
    [SerializeField] private float failureMonologueDuration = 3f;
    [SerializeField] private int failureAttemptsBeforeVideo = 3;
    [SerializeField] private VideoPlaybackConfig failureVideoConfig;
    [SerializeField] private EndCreditConfig failureCredits;

    [Header("Audio")]
    [SerializeField] private AudioSource activationAudioSource;
    [SerializeField] private AudioSource successAudioSource;
    [SerializeField] private AudioSource failureAudioSource;

    private Collider triggerCollider;
    private bool triggerReady;
    private bool isSequenceRunning;
    private bool hasActivatedSuccessfully;
    private int consecutiveFailures;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        UpdateTriggerState(false);
    }

    private void Update()
    {
        TheDistance = PlayerCasting.DistanceFromTarget;

        if (hasActivatedSuccessfully || isSequenceRunning)
        {
            return;
        }

        bool shouldEnable = AreAllSlotsFilled();
        if (shouldEnable != triggerReady)
        {
            UpdateTriggerState(shouldEnable);
        }
    }

    private IEnumerator PlayVideoIfConfigured(VideoPlaybackConfig config)
    {
        if (config == null || config.videoPlayer == null || config.clip == null)
        {
            yield break;
        }

        if (config.objectsToDisable != null)
        {
            foreach (var obj in config.objectsToDisable)
            {
                if (obj != null) obj.SetActive(false);
            }
        }

        if (config.playerMovementToDisable != null) config.playerMovementToDisable.enabled = false;
        if (config.crosshairToHide != null) config.crosshairToHide.SetActive(false);

        if (config.fullscreenUI != null)
        {
            config.fullscreenUI.SetActive(true);
        }

        config.videoPlayer.clip = config.clip;
        config.videoPlayer.Prepare();
        while (!config.videoPlayer.isPrepared)
        {
            yield return null;
        }

        config.videoPlayer.Play();
        while (config.videoPlayer.isPlaying)
        {
            yield return null;
        }

        config.videoPlayer.Stop();

        if (config.fullscreenUI != null)
        {
            config.fullscreenUI.SetActive(false);
        }

        if (config.playerMovementToDisable != null) config.playerMovementToDisable.enabled = true;
        if (config.crosshairToHide != null) config.crosshairToHide.SetActive(true);

        if (config.objectsToDisable != null)
        {
            foreach (var obj in config.objectsToDisable)
            {
                if (obj != null) obj.SetActive(true);
            }
        }
    }

    private void OnMouseOver()
    {
        if (hasActivatedSuccessfully)
        {
            HideUI();
            return;
        }

        if (!triggerReady || isSequenceRunning)
        {
            ShowNeedItemsPrompt();
            return;
        }

        bool inRange = TheDistance <= interactionRange;
        if (!inRange)
        {
            HideUI();
            return;
        }

        ActionDisplay?.SetActive(true);
        ActionTextActivate?.SetActive(true);
        NeedAllItemsText?.SetActive(false);
        ExtraCross?.SetActive(true);

        if (Input.GetButtonDown("Action"))
        {
            PlayAudio(activationAudioSource);
            StartCoroutine(HandleActivationSequence());
        }
    }

    private void OnMouseExit()
    {
        HideUI();
    }

    private IEnumerator HandleActivationSequence()
    {
        isSequenceRunning = true;
        UpdateTriggerState(false);
        HideUI();

        bool allCorrect = AreAllSlotsCorrect();

        PlaySmallRingAnimations();

        yield return new WaitForSeconds(bigRingDelay);

        if (allCorrect)
        {
            PlayBigRingAnimation();
            PlayAudio(successAudioSource);
            hasActivatedSuccessfully = true;
            consecutiveFailures = 0;

            if (!string.IsNullOrWhiteSpace(successMonologue))
            {
                MonologueManager.PlayMonologue(successMonologue, successMonologueDuration, true, true);
            }

            if (successVideoDelayAfterMonologue > 0f)
            {
                yield return new WaitForSeconds(successVideoDelayAfterMonologue);
            }

            MusicTrigger.StopAllMachineRoomAudio();
            SuccessVideoStarted?.Invoke();
            yield return PlayVideoIfConfigured(successVideoConfig);
            yield return ShowCredits(successCredits);
        }
        else
        {
            StopSmallRingAnimations();
            PlayAudio(failureAudioSource);
            if (!string.IsNullOrWhiteSpace(failureMonologue))
            {
                MonologueManager.PlayMonologue(failureMonologue, failureMonologueDuration, true, false);
            }

            consecutiveFailures++;
            if (consecutiveFailures >= Mathf.Max(1, failureAttemptsBeforeVideo))
            {
                yield return PlayVideoIfConfigured(failureVideoConfig);
                yield return ShowCredits(failureCredits);
                consecutiveFailures = 0;
            }
        }

        isSequenceRunning = false;

        if (!hasActivatedSuccessfully)
        {
            UpdateTriggerState(AreAllSlotsFilled());
        }
    }

    private void UpdateTriggerState(bool enable)
    {
        triggerReady = enable;
        if (triggerCollider != null)
        {
            triggerCollider.enabled = enable;
        }

        if (!enable)
        {
            HideUI();
        }
    }

    private bool AreAllSlotsFilled()
    {
        if (slots == null || slots.Length == 0)
        {
            return false;
        }

        foreach (var slot in slots)
        {
            if (slot == null || slot.placedImportantItem == null)
            {
                return false;
            }
        }

        return true;
    }

    private bool AreAllSlotsCorrect()
    {
        if (slots == null || slots.Length == 0)
        {
            return false;
        }

        foreach (var slot in slots)
        {
            if (slot == null || slot.placedImportantItem == null || !slot.isCorrectItemPlaced)
            {
                return false;
            }
        }

        return true;
    }

    private void PlayAudio(AudioSource source)
    {
        if (source == null)
        {
            return;
        }

        source.Stop();
        source.Play();
    }

    private void PlaySmallRingAnimations()
    {
        if (smallRingAnimations == null)
        {
            return;
        }

        foreach (var anim in smallRingAnimations)
        {
            if (anim == null) continue;
            anim.Stop();
            anim.Rewind();
            anim.Play();
        }
    }

    private void StopSmallRingAnimations()
    {
        if (smallRingAnimations == null)
        {
            return;
        }

        foreach (var anim in smallRingAnimations)
        {
            if (anim == null) continue;
            anim.Stop();
            anim.Rewind();
        }
    }

    private void PlayBigRingAnimation()
    {
        if (bigRingAnimation == null)
        {
            return;
        }

        bigRingAnimation.Stop();
        bigRingAnimation.Rewind();
        bigRingAnimation.Play();
    }

    private void HideUI()
    {
        ExtraCross?.SetActive(false);
        ActionDisplay?.SetActive(false);
        ActionTextActivate?.SetActive(false);
        NeedAllItemsText?.SetActive(false);
    }

    private void ShowNeedItemsPrompt()
    {
        ActionDisplay?.SetActive(false);
        ActionTextActivate?.SetActive(false);
        ExtraCross?.SetActive(false);
        if (NeedAllItemsText != null)
        {
            NeedAllItemsText.SetActive(true);
        }
    }

    private IEnumerator ShowCredits(EndCreditConfig config)
    {
        if (config == null || config.slides == null || config.slides.Length == 0)
        {
            yield break;
        }

        for (int i = 0; i < config.slides.Length; i++)
        {
            var slide = config.slides[i];
            if (slide == null || slide.slideObject == null)
            {
                continue;
            }

            slide.slideObject.SetActive(true);
            PlaySlideAnimation(slide);
            float waitTime = Mathf.Max(0f, slide.displayDuration);
            bool isLastSlide = i == config.slides.Length - 1;
            if (!isLastSlide && waitTime > 0f)
            {
                yield return new WaitForSeconds(waitTime);
            }

            if (!isLastSlide && config.deactivateSlideAfterDelay)
            {
                slide.slideObject.SetActive(false);
            }
        }

        if (config.extraDelayAfterSlides > 0f)
        {
            yield return new WaitForSeconds(config.extraDelayAfterSlides);
        }

        if (config.allowReturnToScene && !string.IsNullOrEmpty(config.returnSceneName))
        {
            float wait = Mathf.Max(0f, config.waitBeforeAllowReturn);
            if (wait > 0f)
            {
                yield return new WaitForSeconds(wait);
            }

            yield return WaitForLeftClickAndLoadScene(config.returnSceneName);
        }
    }

    private void PlaySlideAnimation(EndCreditSlide slide)
    {
        if (slide == null || slide.slideObject == null)
        {
            return;
        }

        var animation = slide.slideObject.GetComponent<Animation>();
        if (animation != null)
        {
            animation.Stop();
            animation.Play();
        }

        var animator = slide.slideObject.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(0, 0, 0f);
        }
    }

    private IEnumerator WaitForLeftClickAndLoadScene(string sceneName)
    {
        while (!Input.GetMouseButtonDown(0))
        {
            yield return null;
        }

        SceneManager.LoadScene(sceneName);
    }

    [System.Serializable]
    private class VideoPlaybackConfig
    {
        public VideoPlayer videoPlayer;
        public VideoClip clip;
        public GameObject fullscreenUI;
        public MonoBehaviour playerMovementToDisable;
        public GameObject crosshairToHide;
        public GameObject[] objectsToDisable;
    }

    [System.Serializable]
    private class EndCreditConfig
    {
        public EndCreditSlide[] slides;
        public bool deactivateSlideAfterDelay = true;
        public float extraDelayAfterSlides = 0f;
        public bool allowReturnToScene = true;
        public string returnSceneName = "MainMenu_Scene";
        public float waitBeforeAllowReturn = 5f;
    }

    [System.Serializable]
    private class EndCreditSlide
    {
        public GameObject slideObject;
        public float displayDuration = 4f;
    }
}
