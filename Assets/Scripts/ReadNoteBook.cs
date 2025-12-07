using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ReadNotebook : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float TheDistance;
    public GameObject ActionDisplay;   
    public GameObject ActionText;      // "Press E"
    public GameObject NameObject;      // "Notebook"
    public GameObject ExtraCross;      // crosshair phụ (nếu có)
    public float interactionRange = 3f;

    [Header("Note UI")]
    public GameObject notePanel;       // Panel UI full màn hình
    public Image noteImage;            // Image hiển thị tờ giấy
    public Sprite noteSprite;          // notebookpaper.jpg
    public TextMeshProUGUI noteText;   // Text nằm trên tờ giấy
    [TextArea(3, 8)]
    public string noteContent;         // Nội dung bạn muốn viết

    [Header("Log Settings")]
    public bool registerAsClue = true;           // Có ghi vào tab manh mối không
    [TextArea(3, 8)] public string clueLogEntry; // Nội dung hiển thị trong tab manh mối
    public bool registerAsMonologue = false;     // Có ghi vào tab độc thoại không
    [TextArea(3, 8)] public string monologueEntry; // Nội dung hiển thị trong tab độc thoại

    [Header("Monologue Playback")]
    public bool playMonologueOnOpen = false; // Có phát lời thoại sau khi mở note không
    [TextArea(3, 8)] public string monologueVoiceLine; // Nội dung hiển thị qua MonologueManager
    public float monologueDuration = 4f;

    [Header("Sequence Monologue (Shared)")]
    public bool useSequenceMonologue = false;
    public string sequenceKey;
    [TextArea(3, 8)] public string firstSequenceLine;
    [TextArea(3, 8)] public string nextSequenceLine;
    public float sequenceMonologueDuration = 4f;
    public bool sequenceLineAddsToLog = true;

    [Header("Player")]
    public MonoBehaviour playerMovementScript; // script điều khiển nhân vật
    public GameObject crosshair;              // crosshair chính

    public static event System.Action<string> OnNoteOpened;

    private bool isOpen = false;
    private bool hasRegisteredClue = false;
    private bool hasRegisteredMonologue = false;
    private bool hasPlayedMonologueVoiceLine = false;
    private bool hasTriggeredSequenceMonologue = false;
    private bool isPlayerLookingAtNotebook = false;

    private static readonly Dictionary<string, int> sequenceProgress = new();

    public static bool HasSequenceCompleted(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return false;
        return sequenceProgress.TryGetValue(key, out int progress) && progress > 0;
    }

    void Start()
    {
        // Đảm bảo panel tắt lúc bắt đầu game
        if (notePanel != null)
        {
            notePanel.SetActive(false);
        }
    }

    void Update()
    {
        // Cập nhật khoảng cách giống các script khác
        TheDistance = PlayerCasting.DistanceFromTarget;

        bool canInteract = isPlayerLookingAtNotebook && TheDistance <= interactionRange;

        if (canInteract && Input.GetButtonDown("Action"))
        {
            if (isOpen)
            {
                CloseNote();
            }
            else
            {
                OpenNote();
            }
        }

        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseNote();
        }
    }

    void OnMouseOver()
    {
        isPlayerLookingAtNotebook = true;

        if (!isOpen)
        {
            if (TheDistance <= interactionRange)
            {
                ShowInteractionPrompt();
            }
            else
            {
                HideInteractionPrompt();
            }
        }
    }

    void OnMouseExit()
    {
        isPlayerLookingAtNotebook = false;

        if (!isOpen)
        {
            HideInteractionPrompt();
        }
    }

    private void OpenNote()
    {
        isOpen = true;

        Debug.Log("OpenNote: mở notebook");

        // Tắt điều khiển nhân vật & crosshair nếu cần
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (crosshair != null) crosshair.SetActive(false);

        // Bật panel note
        if (notePanel != null) notePanel.SetActive(true);
        if (noteImage != null && noteSprite != null) noteImage.sprite = noteSprite;
        if (noteText != null) noteText.text = noteContent;

        OnNoteOpened?.Invoke(gameObject.name);

        // Đăng ký vào nhật ký manh mối/độc thoại (chỉ 1 lần)
        string clueText = string.IsNullOrWhiteSpace(clueLogEntry) ? noteContent : clueLogEntry;
        if (registerAsClue && !hasRegisteredClue && !string.IsNullOrWhiteSpace(clueText))
        {
            Debug.Log("ReadNotebook: Register clue -> " + clueText);
            ClueLogManager.AddClue(clueText);
            hasRegisteredClue = true;
        }

        if (registerAsMonologue && !hasRegisteredMonologue && !string.IsNullOrWhiteSpace(monologueEntry))
        {
            Debug.Log("ReadNotebook: Register monologue -> " + monologueEntry);
            ClueLogManager.AddMonologue(monologueEntry);
            hasRegisteredMonologue = true;
        }
    }

    private void CloseNote()
    {
        isOpen = false;

        Debug.Log("CloseNote: đóng notebook");

        if (notePanel != null) notePanel.SetActive(false);
        if (playerMovementScript != null) playerMovementScript.enabled = true;
        if (crosshair != null) crosshair.SetActive(true);

        if (playMonologueOnOpen && !hasPlayedMonologueVoiceLine)
        {
            string voiceLine = string.IsNullOrWhiteSpace(monologueVoiceLine)
                ? (!string.IsNullOrWhiteSpace(monologueEntry) ? monologueEntry : noteContent)
                : monologueVoiceLine;

            if (!string.IsNullOrWhiteSpace(voiceLine))
            {
                MonologueManager.PlayMonologue(voiceLine, monologueDuration, false, true);
                hasPlayedMonologueVoiceLine = true;
            }
        }

        if (useSequenceMonologue)
        {
            HandleSequenceMonologue();
        }

        HideInteractionPrompt();
    }

    private void ShowInteractionPrompt()
    {
        if (NameObject != null) NameObject.SetActive(true);
        if (ActionDisplay != null) ActionDisplay.SetActive(true);
        if (ActionText != null) ActionText.SetActive(true);
        if (ExtraCross != null) ExtraCross.SetActive(true);
    }

    private void HideInteractionPrompt()
    {
        if (NameObject != null) NameObject.SetActive(false);
        if (ActionDisplay != null) ActionDisplay.SetActive(false);
        if (ActionText != null) ActionText.SetActive(false);
        if (ExtraCross != null) ExtraCross.SetActive(false);
    }

    private void HandleSequenceMonologue()
    {
        if (hasTriggeredSequenceMonologue) return;
        if (string.IsNullOrWhiteSpace(sequenceKey)) return;

        if (!sequenceProgress.TryGetValue(sequenceKey, out int progress))
        {
            progress = 0;
        }

        string lineToPlay = progress == 0 ? firstSequenceLine : nextSequenceLine;
        if (string.IsNullOrWhiteSpace(lineToPlay))
        {
            sequenceProgress[sequenceKey] = progress + 1;
            hasTriggeredSequenceMonologue = true;
            return;
        }

        MonologueManager.PlayMonologue(lineToPlay, sequenceMonologueDuration, sequenceLineAddsToLog, true);
        sequenceProgress[sequenceKey] = progress + 1;
        hasTriggeredSequenceMonologue = true;
    }
}