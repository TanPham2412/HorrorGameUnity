using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using TMPro;

public class PlayCassette : MonoBehaviour
{
    public float TheDistance;
    public GameObject ActionDisplay;
    public GameObject ActionText;
    public GameObject NameObject;
    public GameObject NoCassetteText;
    public GameObject ExtraCross;
    public VideoPlayer videoPlayer;
    public GameObject fullScreenVideoUI;
    public MonoBehaviour playerMovementScript;
    public GameObject crosshair;
    public PickUpItem flashlightPickUp;

    [Header("Tape Configurations")]
    public List<TapeConfiguration> tapeConfigs = new();

    private bool isPlaying = false;
    private readonly HashSet<ItemType> monologuesQueuedForTapes = new();

    void Start()
    {
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
        if (config == null || config.videoClip == null)
        {
            Debug.LogWarning("PlayCassette: Missing Tape Config or VideoClip.");
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

        // 1. Mở khóa đèn pin (nếu có)
        if (config.unlockFlashlightAfterPlayback)
        {
            UnlockFlashlightPickup();
        }

        // 2. Chạy Độc thoại (Monologue)
        QueueCassetteMonologues(config);

        // 3. Tắt các vật thể cần tắt (Objects To Deactivate) - Đã thêm lại ở đây
        if (config.objectsToDeactivate != null)
        {
            foreach (var go in config.objectsToDeactivate)
            {
                if (go != null) go.SetActive(false);
            }
        }
    }

    [System.Serializable]
    public class MonologueLine
    {
        [TextArea(2, 5)] public string text;
        public float duration = 5f;
    }

    private void QueueCassetteMonologues(TapeConfiguration config)
    {
        if (config == null || config.monologueLines == null || config.monologueLines.Count == 0) return;
        if (!monologuesQueuedForTapes.Add(config.tapeItem)) return;

        foreach (var line in config.monologueLines)
        {
            if (line == null || string.IsNullOrWhiteSpace(line.text)) continue;
            float duration = line.duration > 0f ? line.duration : 4f;
            MonologueManager.PlayMonologue(line.text, duration, config.logMonologuesToLog, config.preventDuplicate);
        }
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
            if (tape != null && tape.disableFlashlightUntilViewed) return true;
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

        [Header("Extras")]
        public GameObject[] objectsToDeactivate;
    }
}