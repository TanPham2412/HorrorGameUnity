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

    [Header("Monologue Settings")]
    public List<MonologueLine> cassetteMonologueLines = new()
    {
        new MonologueLine
        {
            text = "...Cái... cái quái gì vậy? Gã bảo vệ đó... ông ta thấy gì vậy? Tiếng cười đó... không phải con người.",
            duration = 5f
        },
        new MonologueLine
        {
            text = "Mình phải rời khỏi đây ngay lập tức. Không thể ở lại căn phòng này. Nơi này quá tối, mình cần phải tìm xem có cái ĐÈN PIN nào không và tìm thêm một vài MANH MỐI.",
            duration = 5f
        }
    };
    public bool logCassetteMonologuesToLog = true;
    public bool preventCassetteDuplicate = true;

    [Header("Audio Settings")]
    public AudioSource breathingAudioSource; // Nên kéo AudioSource của nhân vật vào đây
    public AudioClip breathingClip;          // Tệp âm thanh thở dốc
    [Range(0f, 1f)] public float breathingVolume = 1f;

    private bool isPlaying = false; // Đang phát video
    private bool hasShownText = false; // Đã hiển thị text sau video chưa
    private bool hasPlayedBreathing = false; // Đã phát tiếng thở dốc sau khi xem băng lần đầu
    private bool hasQueuedCassetteMonologues = false;

    void Start()
    {
        // Lúc đầu chưa xem băng nên khóa nhặt flashlight
        if (flashlightPickUp != null)
        {
            flashlightPickUp.enabled = false;
            Collider col = flashlightPickUp.GetComponent<Collider>();
            if (col != null) col.enabled = false;
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

            // Debug: Kiểm tra giá trị hasVHSTape
            Debug.Log("hasVHSTape = " + GlobalInventory.hasVHSTape);
            NameObject.SetActive(true);
            // Kiểm tra xem có VHSTape không
            if (GlobalInventory.hasVHSTape)
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
        if (Input.GetKeyDown(KeyCode.E) && TheDistance <= 3 && !isPlaying && GlobalInventory.hasVHSTape)
        {
            ExtraCross.SetActive(false);
            ActionDisplay.SetActive(false);
            ActionText.SetActive(false);
            StartCoroutine(PlayCutscene());
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

    IEnumerator PlayCutscene()
    {
        isPlaying = true;
        playerMovementScript.enabled = false;
        if (crosshair != null) crosshair.SetActive(false);
        fullScreenVideoUI.SetActive(true);
        videoPlayer.Play();
        yield return new WaitForSeconds((float)videoPlayer.clip.length);
        videoPlayer.Stop();
        fullScreenVideoUI.SetActive(false);
        playerMovementScript.enabled = true;
        if (crosshair != null) crosshair.SetActive(true);
        isPlaying = false;

        // Phát tiếng thở dốc sau khi xem xong băng (chỉ lần đầu)
        if (!hasPlayedBreathing)
        {
            PlayBreathingSFX();
            hasPlayedBreathing = true;
        }

        // Sau khi xem xong video lần đầu: cho phép nhặt flashlight
        GlobalInventory.canPickupFlashlight = true;
        if (flashlightPickUp != null)
        {
            flashlightPickUp.enabled = true;
            Collider col = flashlightPickUp.GetComponent<Collider>();
            if (col != null) col.enabled = true;
        }
        
        // Hiển thị text sau khi xem video (chỉ lần đầu tiên)
        if (!hasShownText)
        {
            hasShownText = true;
            StartCoroutine(ShowTextAfterVideo());
        }
    }
    
    IEnumerator ShowTextAfterVideo()
    {
        QueueCassetteMonologues();
        yield break;
    }

    private void QueueCassetteMonologues()
    {
        if (hasQueuedCassetteMonologues) return;
        hasQueuedCassetteMonologues = true;

        if (cassetteMonologueLines == null || cassetteMonologueLines.Count == 0) return;

        foreach (var line in cassetteMonologueLines)
        {
            if (line == null || string.IsNullOrWhiteSpace(line.text)) continue;
            MonologueManager.PlayMonologue(line.text, line.duration, logCassetteMonologuesToLog, preventCassetteDuplicate);
        }
    }

    [System.Serializable]
    public class MonologueLine
    {
        [TextArea(2, 5)] public string text;
        public float duration = 5f;
    }

    private void PlayBreathingSFX()
    {
        if (breathingAudioSource != null && breathingClip != null)
        {
            breathingAudioSource.PlayOneShot(breathingClip, breathingVolume);
        }
    }
}