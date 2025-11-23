using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.InputSystem;
using TMPro; 

public class PlayCassette : MonoBehaviour
{
    public float TheDistance;
    public GameObject ActionDisplay;
    public GameObject ActionText;
    public GameObject NameObject;
    public GameObject NoCassetteText; // Text hiển thị khi không có cassette
    public GameObject ExtraCross;
    public VideoPlayer videoPlayer;
    public GameObject fullScreenVideoUI; // Kéo FullScreenVideo (Raw Image) vào đây
    public MonoBehaviour playerMovementScript; // Kéo SCRIPT điều khiển nhân vật vào đây
    public GameObject crosshair; // Kéo crosshair UI vào đây (nếu có)
    public PickUpItem flashlightPickUp; // Kéo FlashLightTrigger (PickUpItem) vào đây

    [Header("Tape Configurations")]
    public List<TapeConfiguration> tapeConfigs = new();

    [Header("Audio Settings")]
    public AudioSource breathingAudioSource; // AudioSource dùng chung, clip & volume nằm ở từng tape

    private bool isPlaying = false; // Đang phát video
    private readonly HashSet<ItemType> monologuesQueuedForTapes = new();
    private readonly HashSet<ItemType> audioPlayedForTapes = new();

    void Start()
    {
        // Nếu có băng yêu cầu xem trước khi nhặt đèn pin thì khóa nhặt
        if (flashlightPickUp != null && AnyTapeLocksFlashlight())
        {
            LockFlashlightPickup();
        }
    }

    void Update()
    {
        if (!isPlaying)
        {
            TheDistance = PlayerCasting.DistanceFromTarget;
        }
    }

    void OnMouseOver()
    {
        if (TheDistance <= 3 && !isPlaying)
        {

            TapeConfiguration activeTape;
            bool hasTape = TryGetActiveTape(out activeTape);

            NameObject.SetActive(true);
            if (hasTape)
            {
                ActionDisplay.SetActive(true);
                ActionText.SetActive(true);
                ExtraCross.SetActive(true);
            }
            else
            {
                NoCassetteText.SetActive(true);
            }
        }
        else
        {
            NameObject.SetActive(false);
            ExtraCross.SetActive(false);
            ActionDisplay.SetActive(false);
            ActionText.SetActive(false);
            NoCassetteText.SetActive(false);
        }

        // Cho phép xem lại nhiều lần nếu vẫn còn băng
        if (Input.GetKeyDown(KeyCode.E) && TheDistance <= 3 && !isPlaying)
        {
            TapeConfiguration activeTape;
            if (TryGetActiveTape(out activeTape))
            {
                ExtraCross.SetActive(false);
                ActionDisplay.SetActive(false);
                ActionText.SetActive(false);
                StartCoroutine(PlayCutscene(activeTape));
            }
        }
    }

    void OnMouseExit()
    {
        NameObject.SetActive(false);
        ExtraCross.SetActive(false);
        ActionDisplay.SetActive(false);
        ActionText.SetActive(false);
        NoCassetteText.SetActive(false);
    }

    IEnumerator PlayCutscene(TapeConfiguration config)
    {
        if (config == null)
        {
            Debug.LogWarning("PlayCassette: Tape configuration is missing.");
            yield break;
        }

        if (config.videoClip == null)
        {
            Debug.LogWarning($"PlayCassette: Tape {config.tapeItem} is missing a VideoClip.");
            yield break;
        }

        isPlaying = true;
        playerMovementScript.enabled = false;
        if (crosshair != null) crosshair.SetActive(false);
        fullScreenVideoUI.SetActive(true);
        videoPlayer.clip = config.videoClip;
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        videoPlayer.Play();
        while (videoPlayer.isPlaying)
        {
            yield return null;
        }

        videoPlayer.Stop();
        fullScreenVideoUI.SetActive(false);
        playerMovementScript.enabled = true;
        if (crosshair != null) crosshair.SetActive(true);
        isPlaying = false;

        TryPlayTapeAudio(config);

        // Sau khi xem xong video lần đầu: cho phép nhặt flashlight nếu cấu hình yêu cầu
        if (config.unlockFlashlightAfterPlayback)
        {
            UnlockFlashlightPickup();
        }

        QueueCassetteMonologues(config);
    }

    private void QueueCassetteMonologues(TapeConfiguration config)
    {
        if (config == null || config.monologueLines == null || config.monologueLines.Count == 0) return;

        if (!monologuesQueuedForTapes.Add(config.tapeItem)) return;

        foreach (var line in config.monologueLines)
        {
            if (line == null || string.IsNullOrWhiteSpace(line.text)) continue;
            MonologueManager.PlayMonologue(line.text, line.duration, config.logMonologuesToLog, config.preventDuplicate);
        }
    }

    [System.Serializable]
    public class MonologueLine
    {
        [TextArea(2, 5)] public string text;
        public float duration = 5f;
    }

    private void TryPlayTapeAudio(TapeConfiguration config)
    {
        if (config == null || !config.playAudioAfterViewing) return;
        if (breathingAudioSource == null || config.audioClip == null) return;
        if (!audioPlayedForTapes.Add(config.tapeItem)) return;

        breathingAudioSource.PlayOneShot(config.audioClip, config.audioVolume);
    }

    private bool TryGetActiveTape(out TapeConfiguration config)
    {
        config = null;
        ItemType? heldItem = GlobalInventory.currentRegularItem;
        if (heldItem == null) return false;

        foreach (var tape in tapeConfigs)
        {
            if (tape == null) continue;
            if (tape.tapeItem == heldItem.Value)
            {
                config = tape;
                return true;
            }
        }

        return false;
    }

    private bool AnyTapeLocksFlashlight()
    {
        foreach (var tape in tapeConfigs)
        {
            if (tape != null && tape.disableFlashlightUntilViewed)
            {
                return true;
            }
        }

        return false;
    }

    private void LockFlashlightPickup()
    {
        if (flashlightPickUp == null) return;

        GlobalInventory.canPickupFlashlight = false;
        flashlightPickUp.enabled = false;
        Collider col = flashlightPickUp.GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    private void UnlockFlashlightPickup()
    {
        if (flashlightPickUp == null) return;

        GlobalInventory.canPickupFlashlight = true;
        flashlightPickUp.enabled = true;
        Collider col = flashlightPickUp.GetComponent<Collider>();
        if (col != null) col.enabled = true;
    }

    [System.Serializable]
    public class TapeConfiguration
    {
        public string displayName = "New Tape";
        public ItemType tapeItem = ItemType.VHSTape;
        public VideoClip videoClip;
        public bool disableFlashlightUntilViewed = true;
        public bool unlockFlashlightAfterPlayback = true;
        public List<MonologueLine> monologueLines = new();
        public bool logMonologuesToLog = true;
        public bool preventDuplicate = true;
        [Header("Audio")]
        public bool playAudioAfterViewing = true;
        public AudioClip audioClip;
        [Range(0f, 1f)] public float audioVolume = 1f;
    }
}